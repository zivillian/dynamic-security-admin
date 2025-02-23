﻿namespace Dynsec;

public class DynsecProtocolException : Exception
{
    public DynsecProtocolException(string error, string details):base($"{error}: {details}")
    {
        Error = error;
    }

    public string Error { get; }
}