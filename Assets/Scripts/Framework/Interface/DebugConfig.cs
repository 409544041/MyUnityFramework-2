﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class DebugConfig
{
    [XmlElement]
    public bool IsDebug;//是否打印Log
    [XmlElement]
    public bool IsBetaServer;//是否测试服生产服
}
