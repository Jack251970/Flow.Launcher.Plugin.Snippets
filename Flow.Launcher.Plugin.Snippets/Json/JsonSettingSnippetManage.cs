﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flow.Launcher.Plugin.Snippets.Json;

public class JsonSettingSnippetManage : SnippetManage
{
    private readonly PluginInitContext _context;
    private readonly JsonSetting _jsonSetting;
    private readonly ObservableCollection<SnippetModel> _snippets;

    public JsonSettingSnippetManage(PluginInitContext context)
    {
        _context = context;
        _jsonSetting = _context.API.LoadSettingJsonStorage<JsonSetting>();
        _snippets = _jsonSetting.SnippetList;
    }

    /// <summary>
    /// fuzzy search
    /// </summary>
    /// <param name="search">input search text</param>
    /// <param name="key">raw text</param>
    /// <returns></returns>
    private bool _fuzzySearch(string search, string key)
    {
        if (string.IsNullOrEmpty(search)) return true;
        var mr = _context.API.FuzzySearch(search, key);
        return mr.Success;
    }

    private bool _containsSearch(string search, string key)
    {
        return key.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private bool _filter(string search, string key)
    {
        if (_context != null)
        {
            return _fuzzySearch(search, key);
        }

        return _containsSearch(search, key);
    }

    public SnippetModel GetByKey(string key)
    {
        return _snippets.FirstOrDefault(x => x.Key == key);
    }

    public List<SnippetModel> List(string key = null, string value = null)
    {
        if (key != null && value != null)
        {
            return _snippets.Where(x => _filter(key, x.Key) && _filter(key, x.Value)).ToList();
        }

        if (key != null)
            return _snippets.Where(x => _filter(key, x.Key)).ToList();

        if (value != null)
            return _snippets.Where(x => _filter(value, x.Value)).ToList();

        return _snippets.ToList();
    }

    public bool Add(SnippetModel sm)
    {
        var get = GetByKey(sm.Key);
        if (get != null)
        {
            get.Value = sm.Value;
            get.UpdateTime = DateTime.Now;
            get.Score = sm.Score;
            return true;
        }

        sm.UpdateTime = DateTime.Now;
        _snippets.Add(sm);
        return true;
    }

    public bool RemoveByKey(string key)
    {
        var sm = GetByKey(key);
        if (sm == null)
            return true;
        return _snippets.Remove(sm);
    }

    public bool UpdateByKey(SnippetModel sm)
    {
        var get = GetByKey(sm.Key);
        if (get == null)
        {
            sm.UpdateTime = DateTime.Now;
            _snippets.Add(sm);
            return true;
        }

        get.Value = sm.Value;
        get.UpdateTime = DateTime.Now;
        return true;
    }

    public void Clear()
    {
        _snippets.Clear();
        _context.API.SavePluginSettings();
    }
}