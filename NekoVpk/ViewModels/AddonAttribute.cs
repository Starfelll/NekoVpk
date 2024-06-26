﻿using Avalonia;
using System.Collections.Generic;
using System.IO;
using SteamDatabase.ValvePak;
using ReactiveUI;
using NekoVpk.Core;
using System;

namespace NekoVpk.ViewModels
{
    public enum AddonSource
    {
        Unknown,
        WorkShop,
        Local
    }



    public class AddonAttribute : ViewModelBase
    {

        public static List<AddonAttribute> dirty = [];

        protected AddonInfo AddonInfo;

        protected bool? _Enabled;

        public bool? Enable
        {
            get => _Enabled;
            set
            {
                if (_Enabled != value)
                {
                    _Enabled = value;
                    this.RaisePropertyChanged(nameof(Enable));
                    dirty.Add(this);
                }
            }
        }
        public string FileName { get; }
        public AddonSource Source { get; }

        public string Title { get => AddonInfo.Title ?? FileName; }

        public string? Version { get => AddonInfo.Version; }
        public string? Author { get => AddonInfo.Author; }
        public string? Description { get => AddonInfo.Description; }
        public string? Url
        {
            get
            {
                if (!string.IsNullOrEmpty(WorkShopID))
                {
                    return @"https://steamcommunity.com/sharedfiles/filedetails/?id=" + WorkShopID;
                }
                return AddonInfo.Url0;
            }
        }

        public string? WorkShopID;

        public bool IsSubscribed => Source == AddonSource.WorkShop;

        public string TagsOrde
        {
            get
            {
                string result = "";

                foreach (var tag in Tags)
                {
                    result += tag.Name;
                }
                return result;
            }
        }

        public AssetTag[] Tags { get; set; } = [];

        public DateTime ModificationTime { get; set; }

        DateTime _CreationTime;

        readonly string _Type;

        public string Type { get =>  _Type; }

        public DateTime CreationTime { get => _CreationTime; 
            set {
                if (this.RaiseAndSetIfChanged(ref _CreationTime, value) == value)
                {
                    CreationTimeStr = value.ToString();
                    this.RaisePropertyChanged(nameof(CreationTimeStr));
                }
            } 
        }

        public string CreationTimeStr { get; set; }

        public AddonAttribute(bool? enable, string fileName, AddonSource source, AddonInfo addonInfo, string? types = null)
        {
            _Enabled = enable;
            FileName = fileName;
            Source = source;
            AddonInfo = addonInfo;
            _Type = types ?? string.Empty;
        }

        public string GetAbsolutePath(string gameDir)
        {
            string path = Path.Join(gameDir, "addons");

            if (Source == AddonSource.WorkShop)
            {
                path = Path.Join(path, "workshop");
            }

            path = Path.Join(path, FileName);
            return path;
        }

        public Package LoadPackage(string gameDir)
        {
            Package package = new();
            package.Read(GetAbsolutePath(gameDir));
            return package;
        }

        public void ScanContent(Package pak)
        {

        }
    }
}
