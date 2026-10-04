using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
// Minimal deterministic engine substitutes for portable LOGIC tests only.
// This is not Unity, and does not prove Unity compilation, rendering or input.
using Core.GridPawns.Enum;
namespace UnityEngine
{
    public class SerializeField : System.Attribute { }
    public class CreateAssetMenuAttribute : System.Attribute { public string fileName; public string menuName; }
    public class MonoBehaviour { }
    public class ScriptableObject { }
    public class Sprite { }
    public class Texture { }
    public class SpriteRenderer { public Sprite sprite; public bool enabled; }
    public class Transform { public Vector3 position; }
    public struct Vector2
    {
        public float x,y;
        public Vector2(float x,float y) { this.x=x; this.y=y; }
        public static implicit operator Vector3(Vector2 v)=>new Vector3(v.x,v.y,0);
    }
    public struct Vector2Int : System.IEquatable<Vector2Int>
    {
        public int x,y;
        public Vector2Int(int x,int y) { this.x=x; this.y=y; }
        public bool Equals(Vector2Int b)=>x==b.x && y==b.y;
        public override bool Equals(object b)=>b is Vector2Int v && Equals(v);
        public override int GetHashCode()=>System.HashCode.Combine(x,y);
        public static Vector2Int operator +(Vector2Int a,Vector2Int b)=>new Vector2Int(a.x+b.x,a.y+b.y);
        public static bool operator ==(Vector2Int a,Vector2Int b)=>a.Equals(b);
        public static bool operator !=(Vector2Int a,Vector2Int b)=>!a.Equals(b);
    }
    public struct Vector3
    {
        public float x,y,z;
        public Vector3(float x,float y,float z=0) { this.x=x; this.y=y; this.z=z; }
        public static implicit operator Vector2(Vector3 v)=>new Vector2(v.x,v.y);
        public static Vector3 negativeInfinity=>new Vector3(float.NegativeInfinity,float.NegativeInfinity,float.NegativeInfinity);
        public static float Distance(Vector3 a,Vector3 b)=>System.MathF.Sqrt((a.x-b.x)*(a.x-b.x)+(a.y-b.y)*(a.y-b.y)+(a.z-b.z)*(a.z-b.z));
    }
    public struct Rect
    {
        public float x,y,width,height;
        public Rect(float x,float y,float width,float height) { this.x=x;this.y=y;this.width=width;this.height=height; }
        public Vector2 center=>new Vector2(x+width/2,y+height/2);
    }
    public static class Mathf { public static float Max(float a,float b)=>System.MathF.Max(a,b); }
    public static class Debug { public static void Log(object x) { } public static void LogError(object x)=>throw new System.Exception(x.ToString()); }
    public static class Random
    {
        private static readonly System.Random Generator=new System.Random(42);
        public static float value=>(float)Generator.NextDouble();
        public static int Range(int min,int max)=>Generator.Next(min,max);
    }
    public static class PlayerPrefs
    {
        private static readonly Dictionary<string,string> Values=new Dictionary<string,string>();
        public static bool HasKey(string k)=>Values.ContainsKey(k);
        public static string GetString(string k)=>Values.GetValueOrDefault(k,"");
        public static void SetString(string k,string v)=>Values[k]=v;
        public static int GetInt(string k,int d)=>HasKey(k)?int.Parse(Values[k]):d;
        public static void SetInt(string k,int v)=>Values[k]=v.ToString();
        public static void Save() { }
        public static void DeleteAll()=>Values.Clear();
    }
    public static class Resources
    {
        public static Core.Tasks.TaskSO[] Tasks=System.Array.Empty<Core.Tasks.TaskSO>();
        public static T[] LoadAll<T>(string path)=>Tasks.Cast<T>().ToArray();
    }
}
namespace Core.GridPawns.Effect { public class ProducerEffect { } }
namespace Core.GridPawns
{
    public class GridPawn : UnityEngine.MonoBehaviour
    {
        public UnityEngine.Transform transform=new UnityEngine.Transform();
        public UnityEngine.SpriteRenderer SpriteRenderer=new UnityEngine.SpriteRenderer();
        public object PawnEffect;
        public UnityEngine.Vector2Int Coordinate { get;set; }
        public int Level { get;set; }
        public int MaxLevel { get;set; }
        public virtual System.Enum Type { get;protected set; }
        public void SetAttributes(UnityEngine.Vector2Int c,System.Enum t,int l,int m) { Coordinate=c;Type=t;Level=l;MaxLevel=m; }
        public virtual void ApplyData(Data.GridPawnLevelDataSO d) { }
    }
    public class Appliance : GridPawn
    {
        public ApplianceType ApplianceType { get;set; }
        public override System.Enum Type { get=>ApplianceType;protected set=>ApplianceType=(ApplianceType)value; }
    }
}
namespace AYellowpaper.SerializedCollections
{
    public class SerializedDictionaryAttribute : System.Attribute { public SerializedDictionaryAttribute(string a,string b) { } }
    public class SerializedDictionary<K,V> : Dictionary<K,V> { }
}
