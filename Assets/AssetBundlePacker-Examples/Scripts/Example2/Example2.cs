﻿/***************************************************************
 * Copyright 2016 By Zhang Minglin
 * Author: Zhang Minglin
 * Create: 2016/12/26
 * Note  : 例子 - 如何使用PackageDownloader下载指定的资源包
***************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using zcode.AssetBundlePacker;

public class Example2 : MonoBehaviour {

    /// <summary>
    /// 缓存目录
    /// </summary>
    public const string PATH = "Assets/AssetBundlePacker-Examples/Cache/Version_2/AssetBundle";

    /// <summary>
    /// 需要下载的资源包名
    /// </summary>
    public const string PACKAGE_NAME = "level1";

    /// <summary>
    /// 下载地址(例如：http://127.0.0.1/)
    /// </summary>
    public string URL = "http://127.0.0.1/";

    /// <summary>
    /// 
    /// </summary>
    private int current_stage_ = 0;

    /// <summary>
    /// 
    /// </summary>
    private System.Action[] stage_funcs_;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        stage_funcs_ = new System.Action[6];
        stage_funcs_[0] = OnGUI_PreparationWork;
        stage_funcs_[1] = OnGUI_Example;
    }

    /// <summary>
    ///   MonoBehaviour.OnGUI
    /// </summary>
    void OnGUI()
    {
        if (stage_funcs_[current_stage_] != null)
            stage_funcs_[current_stage_]();
    }

    #region PreparationWork
    const string NOTE = 
        "1. 启动例子会从缓存目录中拷贝例子数据至StreamingAssets目录，拷贝执行前会先清空StreamingAssets目录，请注意先转移此目录下的数据。\n"
      + "2. 请把\"Assets/AssetBundlePacker-Examples/Cache/Server\"目录下的\"AssetBundle\"文件夹放置在有效的文件服务器目录下\n"
      + "3. 例子所有使用的AssetBundle原资源放置于\"Assets/AssetBundlePacker-Examples/Cache/Resources\"";
    /// <summary>
    /// 准备工作
    /// </summary>
    void OnGUI_PreparationWork()
    {
        if (GUI.Button(new Rect(0f, 0f, Screen.width, 40f), "从缓存目录中拷贝例子数据，并启动例子"))
        {
            StartExample();
        }
        
        GUI.color = Color.yellow;
        GUI.Label(new Rect(Screen.width / 2 - 100f, 60f, 100f, 20f), "注意");
        GUI.Label(new Rect(0f, 80f, Screen.width, Screen.height - 80f), NOTE);
    }

    /// <summary>
    /// 启动
    /// </summary>
    void StartExample()
    {
        if (Directory.Exists(zcode.AssetBundlePacker.Common.PATH))
            Directory.Delete(zcode.AssetBundlePacker.Common.PATH, true);
        if (Directory.Exists(zcode.AssetBundlePacker.Common.INITIAL_PATH))
            Directory.Delete(zcode.AssetBundlePacker.Common.INITIAL_PATH, true);
        //拷贝例子资源
        zcode.FileHelper.CopyDirectoryAllChildren(PATH, zcode.AssetBundlePacker.Common.INITIAL_PATH);
        //设定资源加载模式为仅加载AssetBundle资源
        ResourcesManager.LoadPattern = new AssetBundleLoadPattern();
        //设定场景加载模式为仅加载AssetBundle资源
        SceneResourcesManager.LoadPattern = new AssetBundleLoadPattern();

        //切换到示例GUI阶段
        current_stage_ = 1;
    }
    #endregion

    #region Example
    /// <summary>
    ///   状态对应的描述信息
    /// </summary>
    public static readonly string[] STATE_DESCRIBE_TABLE = 
    {
         "",
        "连接服务器",
        "下载资源",
        "下载完成",
        "下载失败",
        "下载取消",
        "下载中断",
    };

    /// <summary>
    /// 更新器
    /// </summary>
    private PackageDownloader downloader_;

    /// <summary>
    /// 
    /// </summary>
    void LaunchDownloader()
    {
        downloader_ = gameObject.GetComponent<PackageDownloader>();
        if (downloader_ == null)
            downloader_ = gameObject.AddComponent<PackageDownloader>();

        List<string> url_group = new List<string>();
        url_group.Add(URL);

        List<string> package_group = new List<string>();
        package_group.Add(PACKAGE_NAME);
        downloader_.StartDownload(url_group, package_group);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI_Example()
    {
        //AssetBundleManager全局单例实例化后会自动启动，此处等待其启动完毕
        if (!AssetBundleManager.Instance.WaitForLaunch())
        {
            OnGUI_Example_Launch();
        }
        else
        {
            if (AssetBundleManager.Instance.IsReady)
                OnGUI_Example_Ready();
            else if (AssetBundleManager.Instance.IsFailed)
                OnGUI_Example_Failed();
        }

    }

    void OnGUI_Example_Launch()
    {
        GUI.Label(new Rect(0f, 0f, 200f, 20f), "AssetBundlePacker is launching！");
    }

    void OnGUI_Example_Ready()
    {
        //启动成功
        GUI.Label(new Rect(0f, 0f, Screen.width, 20f), "AssetBundlePacker launch succeed, Version is " + AssetBundleManager.Instance.Version);
        //下载地址
        GUI.Label(new Rect(0f, 20f, 100f, 20f), "下载地址：");
        URL = GUI.TextField(new Rect(100f, 20f, Screen.width - 100f, 20f), URL);

        //启动
        if (downloader_ == null)
        {
            if (GUI.Button(new Rect(0f, 40f, Screen.width, 30f), "下载资源包(" + PACKAGE_NAME + ")"))
            {
                LaunchDownloader();
            }
        }
        else
        {
            //当前更新阶段
            GUI.Label(new Rect(0, 40f, Screen.width, 20f), STATE_DESCRIBE_TABLE[(int)downloader_.CurrentState]);
            //当前阶段进度
            GUI.HorizontalScrollbar(new Rect(0f, 60f, Screen.width, 30f)
                           , 0f, downloader_.CurrentStateCompleteValue
                           , 0f, downloader_.CurrentStateTotalValue);

            if (!downloader_.IsDone && !downloader_.IsFailed)
            {
                if (GUI.Button(new Rect(0, 80f, Screen.width, 20f), "中断下载"))
                {
                    Debug.Log("Abort Download");
                    downloader_.AbortDownload();
                    Destroy(downloader_);
                }
            }
            else if (downloader_.IsDone)
            {
                if (downloader_.IsFailed)
                {
                    if (GUI.Button(new Rect(0, 80f, Screen.width, 20f), "下载失败，重新开始"))
                    {
                        Destroy(downloader_);
                    }
                }
                else
                    GUI.Label(new Rect(0, 80f, Screen.width, 20f), "下载成功");
            }
        }
    }

    void OnGUI_Example_Failed()
    {
        //启动失败
        GUI.color = Color.red;
        GUI.Label(new Rect(0f, 0f, 200f, 20f), "AssetBundlePacker launch occur error!");
    }
    #endregion

}
