﻿//-----------------------------------------------------------------------------
// Copyright 2017 Old Moat Games. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public enum GifPlayerState
{
    PreProcessing,
    Loading,
    Stopped,
    Playing,
    Error,
    Disabled,
}

public enum GifPath
{
    StreamingAssetsPath,
    PersistentDataPath,
    TemporaryCachePath,
}

[AddComponentMenu("Miscellaneous/GIF Player")]
public class GifPlayer : MonoBehaviour
{
    #region Public Fields

    /// <summary>
    /// Sets the GIF to continue playing from the start after it is finished.
    /// </summary>
    public bool Loop = true;

    /// <summary>
    /// Sets the GIF to automatically start playing after it is loaded.
    /// </summary>
    public bool AutoPlay = true;

    /// <summary>
    /// File used for GIF. Use GifPath to set the base path to StreamingAssetsPath, PersistentDataPath or TemporaryCachePath.
    /// </summary>
    public string FileName = "";

    /// <summary>
    /// Path to the GIF.
    /// </summary>
    public GifPath Path = GifPath.StreamingAssetsPath;

    /// <summary>
    /// Returns the width of the GIF
    /// </summary>
    public int Width
    {
        get { return _gifDecoder == null ? 0 : _gifDecoder.GetFrameWidth(); }
    }

    /// <summary>
    /// Returns the heigth of the GIF
    /// </summary>
    public int Height
    {
        get { return _gifDecoder == null ? 0 : _gifDecoder.GetFrameHeight(); }
    }

    /// <summary>
    /// Sets caching for decoded frames. Reload the GIF for it to take effect.
    /// </summary>
    public bool CacheFrames;

    /// <summary>
    /// Sets buffering for frames. When set all frames are loaded and cached at once. Reload GIF for it to take effect.
    /// </summary>
    public bool BufferAllFrames; // Buffer all frames after awake

    /// <summary>
    /// Sets wether or not the decoder runs in a seperate thread
    /// </summary>
    public bool UseThreadedDecoder = true;

    /// <summary>
    /// Sets whether or not playback speed is independent of Time.timeScale.
    /// </summary>
    public bool OverrideTimeScale = false;

    /// <summary>
    /// Sets time scale of Gif playback.
    /// </summary>
    public float TimeScale = 1;

    /// <summary>
    /// The current target component for the GIF. Reload GIF for it to take effect.
    /// </summary>
    public Component TargetComponent
    {
        get { return _targetComponent; }
        set { _targetComponent = value; }
    }

    /// <summary>
    /// The current target material in the component. Used when there is more than 1 material
    /// </summary>
    public int TargetMaterialNumber
    {
        get { return _targetMaterial; }
        set { _targetMaterial = value; }
    }


    /// <summary>
    /// Current state of the GIF player.
    /// </summary>
    public GifPlayerState State { get; private set; }

    /// <summary>
    /// Texture on which the frame is displayed. Texture should be a RGBA32 texture of the same piel size as the GIF. If no texture specified a new one will be created
    /// </summary>
    public Texture2D GifTexture;

    #endregion

    #region Delegates

    /// <summary>
    /// Called when GIF is ready to play.
    /// </summary>
    public delegate void OnReadyAction();

    public event OnReadyAction OnReady;

    /// <summary>
    /// Called when GIF could not be loaded.
    /// </summary>
    public delegate void OnLoadErrorAction();

    public event OnLoadErrorAction OnLoadError;

    #endregion

    #region Internal fields

    private GifDecoder _gifDecoder; // The GIF decoder
    private bool _hasFirstFrameBeenShown; // Has the first frame of the GIF already been shown
    [SerializeField] private Component _targetComponent; // Target component
    [SerializeField] private int _targetMaterial; // Target material number
    private bool _cacheFrames;
    private bool _bufferAllFrames;
    private bool _useThreadedDecoder;
    private float _secondsTillNextFrame; // Seconds till next frame
    private List<GifDecoder.GifFrame> _cachedFrames; // Cache of all frames that have been decoded
    private GifDecoder.GifFrame CurrentFrame { get; set; } // The current frame that is being displayed
    private int CurrentFrameNumber { get; set; } // The current frame we are at

    private Thread _decodeThread;
    private readonly EventWaitHandle _wh = new AutoResetEvent(false);
    private bool _threadIsCanceled;
    private bool _frameIsReady;
    private readonly object _locker = new object();
    private float _editorPreviousUpdateTime; // Time of previous update in editor

    #endregion

    #region Unity Events

    private void Awake()
    {
        //init the AnimatedGifPlayer
        if (State == GifPlayerState.PreProcessing) Init();
    }

    public void Update()
    {
        //check if we need to update the gif frame
        CheckFrameChange();
    }

    private void OnApplicationQuit()
    {
        EndDecodeThread();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Initializes the component with callbacks that are triggered when loading has finished or has failed.
    /// </summary>
    public void Init()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _cacheFrames = _bufferAllFrames = true; // Buffer and cache all frames in the editor
            _useThreadedDecoder = false; // Don't use the threaded decoder in the editor
        }
        else
        {
            _cacheFrames = CacheFrames;
            _bufferAllFrames = BufferAllFrames;
            _useThreadedDecoder = UseThreadedDecoder;
        }
#else
        _cacheFrames = CacheFrames;
        _bufferAllFrames = BufferAllFrames;
        _useThreadedDecoder = UseThreadedDecoder;
#endif
#if UNITY_WEBGL
            if (_useThreadedDecoder) {
                Debug.LogWarning("Animated GIF Player: Threaded Decoder is not available in WebGL");
                _useThreadedDecoder = false;
            }
#endif

        if (_bufferAllFrames && !_cacheFrames)
            // Don't buffer frames if they are not cached
            _bufferAllFrames = false;

        if (_cacheFrames)
        {
            //init the cache
            _cachedFrames = new List<GifDecoder.GifFrame>();
        }

        // Store the target component
        _targetComponent = GetTargetComponent();

        // Start new decoder
        _gifDecoder = new GifDecoder();

        CurrentFrameNumber = 0;
        _hasFirstFrameBeenShown = false;
        _frameIsReady = false;

        // Set state to disabled
        State = GifPlayerState.Disabled;

        // Start the decoder thread
        StartDecodeThread();

        if (FileName.Length <= 0) return; // Only load if the file name is set

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            StartCoroutine(Load());
        }
        else
        {
            // Do not use a coroutine in the editor
            var e = Load();
            while (e.MoveNext())
            {
            }
        }
#else
            StartCoroutine(Load());
#endif
    }

    /// <summary>
    /// Start playback.
    /// </summary>
    public void Play()
    {
        if (State != GifPlayerState.Stopped)
        {
            Debug.LogWarning("Can't play GIF playback. State is: " + State);
            return;
        }

        State = GifPlayerState.Playing;
    }

    /// <summary>
    /// Pause playback.
    /// </summary>
    public void Pause()
    {
        if (State != GifPlayerState.Playing)
        {
            Debug.LogWarning("Can't pause GIF is not playing. State is: " + State);
            return;
        }

        State = GifPlayerState.Stopped;
    }

    /// <summary>
    /// Returns the number of frames in the GIF. Only shows the number of frames that have been decoded.
    /// </summary>
    /// <returns>
    /// Number of frames
    /// </returns>
    public int GetNumberOfFrames()
    {
        return _gifDecoder == null ? 0 : _gifDecoder.GetFrameCount();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Start loading the GIF.
    /// </summary>
    private IEnumerator Load()
    {
        if (FileName.Length == 0)
        {
            Debug.LogWarning("File name not set");
            yield break;
        }

        // Set status to loading
        State = GifPlayerState.Loading;
        string path;


        // Create file path
        if (FileName.Substring(0, 4) == "http")
        {
            // Get image from web
            path = FileName;
        }
        else
        {
            // Local storage
            string gifPath;
            switch (Path)
            {
                case GifPath.StreamingAssetsPath:
                    gifPath = Application.streamingAssetsPath;
                    break;
                case GifPath.PersistentDataPath:
                    gifPath = Application.persistentDataPath;
                    break;
                case GifPath.TemporaryCachePath:
                    gifPath = Application.temporaryCachePath;
                    break;
                default:
                    gifPath = Application.streamingAssetsPath;
                    break;
            }

#if (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_WEBGL && !UNITY_EDITOR)
                path = System.IO.Path.Combine(gifPath, Uri.EscapeUriString(FileName));
#else
            path = System.IO.Path.Combine("file://" + gifPath, Uri.EscapeUriString(FileName));
#endif
        }

        using (var www = new WWW(path))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error) == false)
            {
                // Gif file could not be loaded from streaming assets
                Debug.LogWarning("File load error.\n" + www.error);
                State = GifPlayerState.Error;
            }
            else
            {
                // Gif file loaded. Pass stream to gif decoder
                lock (_locker)
                {
                    if (_gifDecoder.Read(new System.IO.MemoryStream(www.bytes)) == GifDecoder.Status.StatusOk)
                    {
                        // Gif header was read. Prepare the gif
                        // Set status to preprocessing
                        State = GifPlayerState.PreProcessing;

                        // Create the target texture
                        CreateTargetTexture();

                        // Show the first frame
                        StartDecoder();
                    }
                    else
                    {
                        // Error decoding gif
                        Debug.LogWarning("Error loading gif");
                        State = GifPlayerState.Error;
                        if (OnLoadError != null) OnLoadError();
                    }
                }
            }
        }
    }

    // Create target texture
    private void CreateTargetTexture()
    {
        if (GifTexture != null && _gifDecoder != null && GifTexture.width == _gifDecoder.GetFrameWidth() &&
            GifTexture.height == _gifDecoder.GetFrameWidth()) return; // Target texture already set

        if (_gifDecoder == null || _gifDecoder.GetFrameWidth() == 0 || _gifDecoder.GetFrameWidth() == 0)
        {
            GifTexture = Texture2D.blackTexture;
            return;
        }

        GifTexture = CreateTexture(_gifDecoder.GetFrameWidth(), _gifDecoder.GetFrameHeight());
        GifTexture.hideFlags = HideFlags.HideAndDontSave;
    }


    // Used to determine the target component if not set
    private void SetTexture()
    {
        if (_targetComponent == null) return;

        // SpriteRenderer
        if (_targetComponent is SpriteRenderer)
        {
            var target = (SpriteRenderer)_targetComponent;
            var oldSize = target.size;
            var newSprite = Sprite.Create(GifTexture, new Rect(0.0f, 0.0f, GifTexture.width, GifTexture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            newSprite.name = "Gif Player Sprite";
            newSprite.hideFlags = HideFlags.HideAndDontSave;
            target.sprite = newSprite;
            target.size = oldSize;
            return;
        }

        // Renderer
        if (_targetComponent is Renderer)
        {
            var target = (Renderer)_targetComponent;
            if (target.sharedMaterial == null) return;
            if (target.sharedMaterials.Length > 0 && target.sharedMaterials.Length > _targetMaterial)
            {
                target.sharedMaterials[_targetMaterial].mainTexture = GifTexture;
            }
            else
            {
                target.sharedMaterial.mainTexture = GifTexture;
            }

            return;
        }

        // RawImage
        if (_targetComponent is RawImage)
        {
            var target = (RawImage)_targetComponent;
            target.texture = GifTexture;
            return;
        }

        // Image
        if (_targetComponent is Image)
        {
            var target = (Image)_targetComponent;
            var newSprite = Sprite.Create(GifTexture, new Rect(0.0f, 0.0f, GifTexture.width, GifTexture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            target.sprite = newSprite;
            return;
        }
    }

    // Returns a Renderer or RawImage component
    private Component GetTargetComponent()
    {
        var components = GetComponents<Component>();
        return components.FirstOrDefault(component =>
            component is Renderer || component is RawImage || component is Image);
    }


    // Used to set the frame target in the target component
    private void SetTargetTexture()
    {
        if (GifTexture == null || GifTexture.width != _gifDecoder.GetFrameWidth() ||
            GifTexture.height != _gifDecoder.GetFrameWidth())
            GifTexture = CreateTexture(_gifDecoder.GetFrameWidth(), _gifDecoder.GetFrameHeight());

        GifTexture.hideFlags = HideFlags.HideAndDontSave;


        if (TargetComponent == null) return;

        if (TargetComponent is MeshRenderer)
        {
            var target = (Renderer)TargetComponent;
            if (target.sharedMaterial == null) return;
            if (target.sharedMaterials.Length > 0 && target.sharedMaterials.Length > _targetMaterial)
            {
                target.sharedMaterials[_targetMaterial].mainTexture = GifTexture;
            }
            else
            {
                target.sharedMaterial.mainTexture = GifTexture;
            }
        }

        if (TargetComponent is SpriteRenderer)
        {
            var target = (SpriteRenderer)TargetComponent;
            var newSprite = Sprite.Create(GifTexture, new Rect(0.0f, 0.0f, GifTexture.width, GifTexture.height),
                new Vector2(0.5f, 0.5f));
            newSprite.name = "Gif Player Sprite";
            newSprite.hideFlags = HideFlags.HideAndDontSave;
            target.sprite = newSprite;
        }

        if (TargetComponent is RawImage)
        {
            var target = (RawImage)TargetComponent;
            target.texture = GifTexture;
        }

        if (TargetComponent is Image)
        {
            var target = (Image)TargetComponent;
            var newSprite = Sprite.Create(GifTexture, new Rect(0.0f, 0.0f, GifTexture.width, GifTexture.height),
                new Vector2(0.5f, 0.5f));
            target.sprite = newSprite;
        }
    }

    // Creates the texture used
    private static Texture2D CreateTexture(int width, int height)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, false);
    }

    // Reads and caches all frames
    private void BufferFrames()
    {
        if (_useThreadedDecoder)
        {
            // Threaded
            _wh.Set(); // Signal thread to read the next frame
            return;
        }

        // Not threaded
        lock (_locker)
        {
            while (true)
            {
                // Read a single frame
                _gifDecoder.ReadNextFrame(false);

                if (_gifDecoder.AllFramesRead) break;

                // Get the current frame
                var frame = _gifDecoder.GetCurrentFrame();

                // Add frame to frame cache
                AddFrameToCache(frame);
            }

            _frameIsReady = true;
        }
    }

    // Add a frame to the frame cache
    private void AddFrameToCache(GifDecoder.GifFrame frame)
    {
        // Create a coopy of the data to add to the cache since the frame data array is reused for the next frame
        var copyOfImage = new byte[frame.Image.Length];
        Buffer.BlockCopy(frame.Image, 0, copyOfImage, 0, frame.Image.Length);
        frame.Image = copyOfImage;

        // Add frame to frame list
        lock (_cachedFrames) _cachedFrames.Add(frame);
    }

    // Shows the first frame
    private void StartDecoder()
    {
        if (_bufferAllFrames)
        {
            // Buffer all frames
            BufferFrames();
        }
        else
        {
            // Prepare the next frame
            StartReadFrame();
        }

        //player is ready to start
        State = GifPlayerState.Stopped;

        //the ready event
        if (OnReady != null) OnReady();

#if UNITY_EDITOR
        if (AutoPlay && Application.isPlaying) Play(); //don't start autoplay in the editor
#else
            if (AutoPlay) Play();
#endif
    }

    // Sets the time at which the next frame should be shown
    private void SetNextFrameTime()
    {
        _secondsTillNextFrame = CurrentFrame.Delay;
    }

    // Check if the next frame should be shown
    private void UpdateFrameTime()
    {
        if (State != GifPlayerState.Playing) return; // Not playing

        if (!Application.isPlaying || OverrideTimeScale)
        {
            // Play in editor mode or time is independant from Time.timeScale
            if (OverrideTimeScale)
            {
                _secondsTillNextFrame -= (Time.realtimeSinceStartup - _editorPreviousUpdateTime) * TimeScale;
            }
            else
            {
                _secondsTillNextFrame -= Time.realtimeSinceStartup - _editorPreviousUpdateTime;
            }

            _editorPreviousUpdateTime = Time.realtimeSinceStartup;
            return;
        }

        // Calculate seconds till next gif frame
        _secondsTillNextFrame -= Time.deltaTime;
    }

    // Update the frame
    private void UpdateFrame()
    {
        if (_gifDecoder.NumberOfFrames > 0 && _gifDecoder.NumberOfFrames == CurrentFrameNumber)
        {
            // Set frame number to 0 if we are at the last one
            CurrentFrameNumber = 0;
            if (!Loop)
            {
                // Stop playback if not looping
                Pause();
                return;
            }
        }

        if (_cacheFrames)
        {
            // Frames are cached
            lock (_cachedFrames)
            {
                CurrentFrame = _cachedFrames.Count > CurrentFrameNumber
                    ? _cachedFrames[CurrentFrameNumber]
                    : _gifDecoder.GetCurrentFrame();
            }

            // Prepare the next frame 
            if (!_gifDecoder.AllFramesRead)
            {
                // Not all frames are read yet. Prepare the next frame
                StartReadFrame();
            }
        }
        else
        {
            // Get the frame from the decoder
            CurrentFrame = _gifDecoder.GetCurrentFrame();

            // Prepare the next frame
            StartReadFrame();
        }

        // Update the target texture with the new frame
        UpdateTexture();

        // Set next frame time
        SetNextFrameTime();

        // Move to next frame
        CurrentFrameNumber++;
    }


    // Check if the frame needs to be updated
    private void CheckFrameChange()
    {
        if (State != GifPlayerState.Playing && _hasFirstFrameBeenShown || !_frameIsReady) return;


        if (!_hasFirstFrameBeenShown)
        {
            // Show the first frame
            SetTexture();
            lock (_locker) UpdateFrame();
            _hasFirstFrameBeenShown = true;
            return;
        }

        UpdateFrameTime();

        if (_secondsTillNextFrame > 0) return;

        // Time to change the frame
        lock (_locker) UpdateFrame();
    }

    // Update the target texture
    private void UpdateTexture()
    {
        // Upload texture data
        GifTexture.LoadRawTextureData(CurrentFrame.Image);

        // Apply
        GifTexture.Apply();
    }

    // Starts reading a frame
    private void StartReadFrame()
    {
        _frameIsReady = false;
        if (_useThreadedDecoder)
        {
            // Signal thread to read the next frame
            _wh.Set();
            return;
        }

        // Not threaded
        if (_cacheFrames && _gifDecoder.AllFramesRead)
            return; // Don't retrieve data if we already have cached all frames

        // Read the next frame
        _gifDecoder.ReadNextFrame(!_cacheFrames);

        // Add frame to cache if caching is enabled
        if (_cacheFrames && !_gifDecoder.AllFramesRead) AddFrameToCache(_gifDecoder.GetCurrentFrame());

        // Mark the frame as ready
        _frameIsReady = true;
    }

    // Start the decode thread
    private void StartDecodeThread()
    {
        if (!_useThreadedDecoder || (_decodeThread != null && _decodeThread.IsAlive)) return;

        _threadIsCanceled = false;
        _decodeThread = new Thread(() => FrameDataThread(!_cacheFrames, _bufferAllFrames));
        _decodeThread.Name = "gifDecoder" + _decodeThread.ManagedThreadId;
        _decodeThread.IsBackground = true;
        _decodeThread.Start();
    }

    // Ends the decode thread
    private void EndDecodeThread()
    {
        if (_threadIsCanceled) return;
        _threadIsCanceled = true;
        _wh.Set();
    }

    // The decode thread
    private void FrameDataThread(bool loopDecoder, bool readAllFrames)
    {
        _wh.WaitOne();
        while (!_threadIsCanceled)
        {
            lock (_locker)
            {
                // Read the next frame
                _gifDecoder.ReadNextFrame(loopDecoder);

                if (_cacheFrames && _gifDecoder.AllFramesRead)
                {
                    _frameIsReady = true;
                    break;
                }

                if (_cacheFrames) AddFrameToCache(_gifDecoder.GetCurrentFrame());

                if (readAllFrames)
                {
                    if (_gifDecoder.AllFramesRead)
                    {
                        _frameIsReady = true;
                        break;
                    }

                    continue;
                }

                _frameIsReady = true;
            }

            _wh.WaitOne(); // Wait for signal that frame must be read
        }

        _threadIsCanceled = true;
        _wh.Close();
    }

    #endregion
}