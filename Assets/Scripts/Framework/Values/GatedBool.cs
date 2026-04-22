using System.Collections.Generic;

public class GatedBool
{
    private readonly HashSet<string> vetoes = new HashSet<string>();

    public bool True => vetoes.Count == 0;

    public void Set(bool value, string key)
    {
        if (value) vetoes.Remove(key);
        else vetoes.Add(key);
    }

    public void Clear(string key) => vetoes.Remove(key);
}