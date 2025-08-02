using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Pair<T,U>
{
    public T first;
    public U second;

    public Pair(T fst,U snd)
    {
        first = fst; 
        second = snd;
    }
}
public class Auxiliary 
{

}
