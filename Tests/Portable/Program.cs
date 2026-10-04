using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Core;
using Core.GridPawns;
using Core.GridPawns.Data;
using Core.GridPawns.Enum;
using Core.Helpers;
using Core.Tasks;
using MVP.Models;
using DI;
using UIExtensions;
using UnityEngine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using AYellowpaper.SerializedCollections;

internal static class Program
{
    static int count;
    static void Check(bool condition,string name) { if (!condition) throw new Exception(name); count++; Console.WriteLine("PASS "+name); }
    static void Set(object o,string p,object v)=>o.GetType().GetProperty(p).SetValue(o,v);
    static Appliance Item(int level)=>new Appliance { ApplianceType=ApplianceType.ApplianceA,Level=level,MaxLevel=11 };
    static void Main(string[] args)
    {
        var root=Path.GetFullPath(args.Length>0?args[0]:".");
        int syntaxFiles=0;
        foreach (var path in Directory.GetFiles(Path.Combine(root,"Assets"),"*.cs",SearchOption.AllDirectories))
        {
            foreach(var symbols in new[]{new[]{"UNITY_EDITOR","UNITY_STANDALONE","DOTWEEN"},new[]{"UNITY_IOS","DOTWEEN"},new[]{"UNITY_WEBGL","DOTWEEN"}})
            {
                var tree=CSharpSyntaxTree.ParseText(File.ReadAllText(path),new CSharpParseOptions(LanguageVersion.CSharp9,preprocessorSymbols:symbols),path);
                var errors=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
                if(errors.Length>0) throw new Exception(string.Join("\n",errors.Select(x=>x.ToString())));
            }
            syntaxFiles++;
        }
        Check(true,$"C# 9 syntax: {syntaxFiles} files, editor/iOS/WebGL symbols (not Unity compilation)");
        var producer=new Producer();
        var data=new ProducerLevelDataSO();
        Set(data,"Capacity",10);Set(data,"GeneratedApplianceType",ApplianceType.ApplianceA);
        Set(data,"GeneratingRatioDict",new SerializedDictionary<int,float>{{1,.8f},{2,.2f}});
        producer.ApplyData(data);producer.Capacity=0;
        for(int i=0;i<10000;i++) producer.ReduceCapacity();
        Check(producer.Capacity==int.MaxValue,"10,000 productions and zero-capacity legacy save stay unlimited");
        int ones=0;
        for(int i=0;i<10000;i++) { var n=producer.GetApplianceLevelToProduce(); if(n==1)ones++;else if(n!=2)throw new Exception("Bad generated level"); }
        Check(ones>7700 && ones<8300,"producer retains 80/20 level distribution, deterministic seed 42");
        Check(MergeRules.CanMerge(Item(1),Item(1)),"equal type and level can merge");
        Check(!MergeRules.CanMerge(Item(1),Item(2)),"different levels cannot merge");
        Check(!MergeRules.CanMerge(Item(11),Item(11)),"maximum level cannot overflow");
        var first=Item(1);Check(!MergeRules.CanMerge(first,first),"self merge rejected");
        Check(!MergeRules.CanMerge(first,new Producer { Level=1,MaxLevel=2,ProducerType=ProducerType.ProducerA }),"different pawn families cannot merge");
        var grid=new GridPawn[8,8];grid[4,4]=producer;
        Check(GridPositionHelper.FindClosestEmptyCoordinate(new Vector2Int(4,4),grid)==new Vector2Int(5,4),"production chooses nearest empty cell");
        for(int x=0;x<8;x++)for(int y=0;y<8;y++)grid[x,y]=Item(1);
        Check(GridPositionHelper.FindClosestEmptyCoordinate(new Vector2Int(4,4),grid)==null,"full board blocks production");
        var goals=new List<Goal>{new Goal{ApplianceType=ApplianceType.ApplianceA,Level=3},new Goal{ApplianceType=ApplianceType.ApplianceA,Level=3}};
        grid=new GridPawn[8,8];grid[0,0]=Item(3);
        Check(TaskRequirementMatcher.Match(grid,goals)==null,"duplicate order goals need distinct items");
        grid[1,0]=Item(3);
        var matched=TaskRequirementMatcher.Match(grid,goals);
        Check(matched?.Count==2 && matched[0]!=matched[1],"two distinct matching items fulfill two goals");
        grid[0,0]=null;Check(TaskRequirementMatcher.Match(grid,goals)==null,"revalidation rejects an item consumed by another order");
        grid[0,0]=Item(11);
        Check(TaskRequirementMatcher.Match(grid,new[]{new Goal{ApplianceType=ApplianceType.ApplianceA,Level=11}})?.Count==1,"max-level item fulfills final order");
        PlayerPrefs.DeleteAll();LocalResources.Initialize();Check(LocalResources.Coins==99999999 && LocalResources.HasUnlimitedEnergy,"initial local coins and unlimited energy");
        PlayerPrefs.SetInt("Sandbox.Coins",123);LocalResources.Initialize();Check(LocalResources.Coins==123,"wallet initialization preserves existing save");
        var task1=new TaskSO();Set(task1,"TaskID",1);var task2=new TaskSO();Set(task2,"TaskID",2);
        Resources.Tasks=new[]{task2,task1};var tasks=new TaskModel();Check(tasks.GetNextTask().TaskID==1 && tasks.GetNextTask().TaskID==2 && tasks.GetNextTask()==null,"orders load by ID with finite upstream queue");
        tasks.CompleteTask(1);tasks.CompleteTask(1);var reload=new TaskModel();Check(reload.GetNextTask().TaskID==2 && reload.GetNextTask()==null,"order completion survives reload without duplicate completion");
        GridPositionHelper.ClearPositions();GridPositionHelper.CalculateItemWorldPosition(new Vector3(0,0,0),new Vector2(1,1),new Vector2Int(0,0),1);
        GridPositionHelper.ClearPositions();GridPositionHelper.CalculateItemWorldPosition(new Vector3(10,10,0),new Vector2(1,1),new Vector2Int(0,0),1);
        Check(GridPositionHelper.GetWorldPositionFromCoordinate(new Vector2Int(0,0)).x==11,"board position cache refreshes on scene reload");
        var di=new Container();var service=new Service();di.BindAsSingle(()=>service);di.Resolve<Service>();di.Resolve<Service>();Check(service.Initializations==1,"DI initializes shared factories only once");
        foreach(var size in new[]{(768,1024),(810,1080),(834,1194),(820,1180),(1024,1366),(744,1133),(390,844)})
        {
            var safe=new Rect(0,24,size.Item1,size.Item2-58);
            var area=TabletLayoutMath.BoardArea(safe,size.Item1,size.Item2);
            var camera=TabletLayoutMath.CameraSize(new Vector2(9.53f,9.68f),area,(float)size.Item1/size.Item2);
            var pixelsPerWorldUnit=size.Item2/(2*camera);
            Check(9.53f*pixelsPerWorldUnit<=area.width*size.Item1 && 9.68f*pixelsPerWorldUnit<=area.height*size.Item2,$"board fits safe viewport {size.Item1}x{size.Item2}");
            Check(1.16f*pixelsPerWorldUnit>=44,$"board cell >=44 logical points {size.Item1}x{size.Item2}");
        }
        Console.WriteLine($"TOTAL {count} checks PASS. Unity editor/player checks remain separate.");
    }
    class Service : IPreInitializable { public int Initializations;public void PreInitialize()=>Initializations++; }
}
