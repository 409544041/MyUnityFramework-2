using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class ScreenshotManager : SingletonMono<ScreenshotManager>
{
    private Coroutine coroutine;
    /// <summary>
    ///  ��ȡ��Ļ���أ�������Ч���Զ����С
    /// </summary>
    public void TakePhoto(string path, Size size, Action<Texture2D, string> callback = null)
    {
        if (coroutine == null)
        {
            coroutine = StartCoroutine(Shot(path, size, callback));
        }
    }
    /// <summary>
    /// ��ȡĳ�������Ⱦ�Ļ��棬������Ч���Զ����С
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="size"></param>
    public void TakePhoto(Camera camera, string path, Size size, Action<Texture2D, string> callback = null)
    {
        if (coroutine == null)
        {
            coroutine = StartCoroutine(Shot(camera, path, size, callback));
        }
    }
    /// <summary>
    /// ����(��ȡĳ�������Ⱦ�Ļ���)
    /// </summary>
    /// <returns></returns>
    private IEnumerator Shot(Camera camera, string path, Size size, Action<Texture2D, string> callback = null)
    {
        Texture2D texture = CaptureCamera(camera, new Rect(Screen.width / 2 - size.w / 2, Screen.height / 2 - size.h / 2, size.w, size.h));
        yield return new WaitForEndOfFrame();
        var img = texture.EncodeToPNG();
        if (!FileManager.FolderExist(path))//��������Ŀ¼������������򴴽�Ŀ¼  
        {
            FileManager.CreateFolder(path);
        }
        string imageName = string.Format("Image{0}.png", Utils.GetTimeStamp);
        string file = string.Format("{0}/{1}", path, imageName);
        File.WriteAllBytes(file, img);
        yield return new WaitForEndOfFrame();
        coroutine = null;
        callback?.Invoke(texture, imageName);
    }
    /// <summary>
    /// ����(��ȡ��Ļ����)
    /// </summary>
    /// <returns></returns>
    private IEnumerator Shot(string path, Size size, Action<Texture2D, string> callback = null)
    {
        Texture2D texture = new Texture2D(size.w, size.h, TextureFormat.RGB24, false);
        yield return new WaitForEndOfFrame();
        texture.ReadPixels(new Rect(Screen.width / 2 - size.w / 2, Screen.height / 2 - size.h / 2, size.w, size.h), 0, 0, false);
        texture.Apply();
        var img = texture.EncodeToPNG();
        if (!FileManager.FolderExist(path))//��������Ŀ¼������������򴴽�Ŀ¼  
        {
            FileManager.CreateFolder(path);
        }
        string imageName = string.Format("Image{0}.png", Utils.GetTimeStamp);
        string file = string.Format("{0}/{1}", path, imageName);
        File.WriteAllBytes(file, img);
        yield return new WaitForEndOfFrame();
        coroutine = null;
        callback?.Invoke(texture, imageName);
    }
    /// <summary>  
    /// �������ͼ��   
    /// </summary>  
    /// <returns>The screenshot2.</returns>  
    /// <param name="camera">Camera.Ҫ�����������</param>  
    /// <param name="rect">Rect.����������</param>  
    private Texture2D CaptureCamera(Camera camera, Rect rect)
    {
        // ����һ��RenderTexture����  
        RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0);
        // ��ʱ������������targetTextureΪrt, ���ֶ���Ⱦ������  
        camera.targetTexture = rt;
        camera.Render();
        //ps: --- ����������ϵڶ������������ʵ��ֻ��ͼĳ����ָ�������һ�𿴵���ͼ��  
        //ps: camera2.targetTexture = rt;  
        //ps: camera2.Render();  
        //ps: -------------------------------------------------------------------  
        // �������rt, �������ж�ȡ���ء�  
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);// ע�����ʱ�����Ǵ�RenderTexture.active�ж�ȡ����  
        screenShot.Apply();
        // ������ز�������ʹ��camera��������Ļ����ʾ  
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;  
        RenderTexture.active = null; // JC: added to avoid errors  
        GameObject.Destroy(rt);
        return screenShot;
    }
}
/// <summary>
/// ��С
/// </summary>
[Serializable]
public struct Size
{
    public int w;
    public int h;
    public Size(int width, int height)
    {
        w = width;
        h = height;
    }
}