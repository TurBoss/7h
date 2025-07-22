using Iros._7th.Workshop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;

namespace Iros._7th.Workshop.ConfigSettings
{
    public class tnx3000ConfigManager
    {
        private string _pathTotnx3000Toml = Sys.PathTotnx3000Toml;
        private string _pathTotnx3000TomlBak = Path.Combine(Sys.PathToGameDriverFolder, "tnx3000.toml.bak");
        private string _pathTotnx3000TomlTmp = Path.Combine(Sys.PathToGameDriverFolder, "tnx3000.toml.tmp");
        private Exception _lastException = null;

        private TomlTable _toml = null;

        public tnx3000ConfigManager()
        {
            Reload();
        }

        public Exception GetLastError()
        {
            Exception ret = _lastException;

            _lastException = null;

            return ret;
        }

        public void Reload()
        {
            try
            {
                // first launch the Sys Path was not setup correctly yet so _toml is null
                if (_toml == null) _pathTotnx3000Toml = Sys.PathTotnx3000Toml;

                if (File.Exists(_pathTotnx3000Toml)) _toml = Toml.Parse(File.ReadAllBytes(_pathTotnx3000Toml)).ToModel();
            }
            catch (Exception ex)
            {
                _lastException = ex;
                Sys.Message(new WMessage() { Text = $"Something went wrong while parsing \"{_pathTotnx3000Toml}\"", LoggedException = _lastException, LogLevel = WMessageLogLevel.Error });
            }
        }

        public string Get(string key)
        {
            return _toml[key].ToString();
        }

        public void Set(string key, string value)
        {
            switch (_toml[key])
            {
                case string s:
                    {
                        _toml[key] = value;
                        break;
                    }
                case bool b:
                    {
                        _toml[key] = bool.Parse(value);
                        break;
                    }
                case double d:
                    {
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.NumberDecimalSeparator = ".";

                        _toml[key] = double.Parse(value, NumberStyles.Any, ci);
                        break;
                    }
                case Int64 i:
                    {
                        _toml[key] = Int64.Parse(value);
                        break;
                    }
            }
        }

        public void Set<T>(string key, List<T> value)
        {
            TomlArray ta = new TomlArray();
            foreach (T s in value)
            {
                ta.Add(s);
            }
            _toml[key] = ta;
        }

        public bool IsSetWithValue(string key, string value)
        {
            switch (_toml[key])
            {
                case string s:
                    {
                        if (s != value) return false;
                        break;
                    }
                case bool b:
                    {
                        if (b != bool.Parse(value)) return false;
                        break;
                    }
                case double d:
                    {
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";

                        if (d != double.Parse(value, NumberStyles.Any, ci)) return false;
                        break;
                    }
                case Int64 i:
                    {
                        if (i != Int64.Parse(value)) return false;
                        break;
                    }
            }

            return true;
        }

        public bool HasKey(string key)
        {
            return _toml.ContainsKey(key);
        }

        public void Save()
        {
            List<string> _read = File.ReadAllLines(_pathTotnx3000Toml).ToList();
            List<string> _write = new List<string>();

            foreach (string line in _read)
            {
                string[] parts = line.Split(new[] { "=" }, 2, StringSplitOptions.None);
                if (!line.StartsWith("#") && parts.Length == 2)
                {
                    string settingName = parts[0].Trim();

                    if (_toml.ContainsKey(settingName))
                    {
                        dynamic node = _toml[settingName];

                        switch (node)
                        {
                            case bool b:
                                {
                                    _write.Add($"{settingName} = {b.ToString().ToLower()}");
                                    break;
                                }
                            case string s:
                                {
                                    _write.Add($"{settingName} = \"{s.ToLower()}\"");
                                    break;
                                }
                            case double d:
                                {
                                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                                    ci.NumberFormat.NumberDecimalSeparator = ".";

                                    _write.Add($"{settingName} = {((double)node).ToString("0.0", ci)}");
                                    break;
                                }
                            case Int64 i:
                                {
                                    if (settingName == "devtools_hotkey")
                                    {
                                        _write.Add($"{settingName} = 0x{i.ToString("X")}");
                                    }
                                    else
                                    {
                                        _write.Add($"{settingName} = {i.ToString()}");
                                    }
                                    break;
                                }
                            case TomlArray ta:
                                {
                                    if (ta.Count > 0)
                                    {
                                        if (node[0].GetType() == typeof(string))
                                        {
                                            _write.Add($"{settingName} = [\"{string.Join("\", \"", ta.Select(w => w.ToString()))}\"]");
                                        }
                                        else
                                        {
                                            _write.Add($"{settingName} = [{string.Join(", ", ta.Select(w => w.ToString()))}]");
                                        }
                                    }
                                    else
                                    {
                                        _write.Add($"{settingName} = []");
                                    }
                                    break;
                                }
                            default:
                                {
                                    _write.Add($"{settingName} = {node.ToString()}");
                                    break;
                                }
                        }
                    }
                    else
                    {
                        _write.Add(line);
                    }
                }
                else
                {
                    _write.Add(line);
                }
            }

            File.WriteAllLines(_pathTotnx3000Toml, _write);
        }

        public void OverrideInternalKeys()
        {
            // first launch the Sys Path was not setup correctly yet so _toml is null
            if (_toml == null)
            {
                _pathTotnx3000Toml = Sys.PathTotnx3000Toml;
                Reload();
            }
            // Override known internal keys on save to preserve mod behavior override logic
            _toml["external_sfx_path"] = "sfx";
            _toml["external_sfx_ext"] = "ogg";
            _toml["external_music_path"] = "data/music_ogg";
            _toml["use_external_music"] = true;
            _toml["external_music_ext"] = "ogg";
            _toml["external_voice_path"] = "voice";
            _toml["external_voice_ext"] = "ogg";
            _toml["external_ambient_path"] = "ambient";
            _toml["external_ambient_ext"] = "ogg";
            _toml["external_mesh_path"] = "mesh";
            _toml["ffmpeg_video_ext"] = "avi";
            _toml["mod_path"] = "mods/Textures";
            _toml["direct_mode_path"] = "direct";
        }

        public void ResetTo7thHeavenDefaults()
        {
            ConfigSpec gameDriverUISpec = Util.Deserialize<Iros._7th.Workshop.ConfigSettings.ConfigSpec>(Sys.PathToGameDriverUiXml());

            foreach (Setting item in gameDriverUISpec.Settings)
            {
                string[] parts = item.DefaultValue.Split(',');
                foreach (string p in parts)
                {
                    string trimmedParts = p.Trim();

                    string[] set = trimmedParts.Split('=');
                    if (set.Length == 2)
                    {
                        string trimmedName = set[0].Trim();
                        string trimmedVal = set[1].Trim();

                        Set(trimmedName, trimmedVal);
                    }
                }
            }
        }

        public void Backup(bool isTemporary = false)
        {
            string _path = isTemporary ? _pathTotnx3000TomlTmp : _pathTotnx3000TomlBak;

            if (File.Exists(_pathTotnx3000Toml))
            {
                if (File.Exists(_path)) File.Delete(_path);

                File.Copy(_pathTotnx3000Toml, _path);
            }
        }

        public void RestoreBackup(bool isTemporary = false)
        {
            string _path = isTemporary ? _pathTotnx3000TomlTmp : _pathTotnx3000TomlBak;

            if (File.Exists(_path))
            {
                File.Delete(_pathTotnx3000Toml);
                File.Copy(_path, _pathTotnx3000Toml);
                Reload();
            }
        }
    }
}