/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Tomlyn;
using Tomlyn.Model;

namespace Iros._7th.Workshop.ConfigSettings {

    public class Settings {
        private TomlTable _toml = null;

        public string Get(string setting) {
            return (string)_toml[setting];
        }
        public bool IsMatched(string spec) {
            string[] parts = spec.Split(',');
            foreach (string p in parts) {
                string trimmedParts = p.Trim();

                string[] set = trimmedParts.Split('=');
                if (set.Length == 2) {
                    string trimmedName = set[0].Trim();
                    string trimmedVal = set[1].Trim();

                    string value = _toml[trimmedName].ToString();
                    if (!trimmedVal.Equals(value ?? String.Empty, StringComparison.InvariantCultureIgnoreCase))
                        return false;
                }                    
            }
            return true;
        }
        public void Apply(string spec) {
            if (String.IsNullOrWhiteSpace(spec)) return;
            string[] parts = spec.Split(',');
            foreach (string p in parts) {
                string trimmedParts = p.Trim();
                string[] set = trimmedParts.Split('=');

                if (set.Length == 2) {
                    string trimmedName = set[0].Trim();
                    string trimmedVal = set[1].Trim();

                    if (trimmedName.StartsWith("speedhack_"))
                    {
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";

                        _toml[trimmedName] = double.Parse(trimmedVal, NumberStyles.Any, ci);
                        break;
                    }
                    else
                    {
                        switch (_toml[trimmedName])
                        {
                            case string s:
                                {
                                    _toml[trimmedName] = trimmedVal;
                                    break;
                                }
                            case bool b:
                                {
                                    _toml[trimmedName] = bool.Parse(trimmedVal);
                                    break;
                                }
                            case double d:
                                {
                                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                                    ci.NumberFormat.CurrencyDecimalSeparator = ".";

                                    _toml[trimmedName] = double.Parse(trimmedVal, NumberStyles.Any, ci);
                                    break;
                                }
                            case Int64 i:
                                {
                                    _toml[trimmedName] = Int64.Parse(trimmedVal);
                                    break;
                                }
                        }
                    }
                }
            }
        }

        public bool HasSetting(string spec)
        {
            List<bool> exists = new List<bool>();

            string[] parts = spec.Split(',');
            foreach (string p in parts)
            {
                string trimmedParts = p.Trim();

                string[] set = trimmedParts.Split('=');
                if (set.Length == 2)
                {
                    string trimmedName = set[0].Trim();
                    string trimmedVal = set[1].Trim();

                    exists.Add(_toml.ContainsKey(trimmedName));
                }
            }

            return exists.Count > 0 && exists.All(s => s);
        }

        public Settings(string _filePath) {
            _toml = Toml.Parse(File.ReadAllBytes(_filePath)).ToModel();
        }

        private void OverrideKnownInternals()
        {
            _toml["external_sfx_path"] = "sfx";
            _toml["external_sfx_ext"] = "ogg";
            _toml["external_music_path"] = "music/vgmstream";
            _toml["external_music_ext"] = "ogg";
            _toml["external_voice_path"] = "voice";
            _toml["external_voice_ext"] = "ogg";
            _toml["ffmpeg_video_ext"] = "avi";
            _toml["mod_path"] = "mods/Textures";
            _toml["direct_mode_path"] = "direct";
        }

        /// <summary>
        /// Adds any missing default settings
        /// </summary>
        public void SetMissingDefaults(List<Setting> settings)
        {
            foreach (Setting item in settings)
            {
                if (!HasSetting(item.DefaultValue))
                {
                    Apply(item.DefaultValue);
                }
            }
        }

        public void Save(string _filePath) {
            List<string> _read = File.ReadAllLines(_filePath).ToList();
            List<string> _write = new List<string>();

            // Override known internal keys on save to preserve mod behavior override logic
            OverrideKnownInternals();

            foreach (string line in _read)
            {
                string[] parts = line.Split(new[] { "=" }, 2, StringSplitOptions.None);
                if (!line.StartsWith("#") && parts.Length == 2)
                {
                    string settingName = parts[0].Trim();

                    if (_toml.ContainsKey(settingName))
                    {
                        dynamic node = _toml[settingName];

                        if (settingName.StartsWith("speedhack_"))
                        {
                            _write.Add($"{settingName} = {node.ToString().Replace(",", ".")}");
                        }
                        else
                        {
                            switch (node)
                            {
                                case bool b:
                                    {
                                        _write.Add($"{settingName} = \"{ b.ToString().ToLower() }\"");
                                        break;
                                    }
                                case string s:
                                    {
                                        _write.Add($"{settingName} = \"{ s.ToLower() }\"");
                                        break;
                                    }
                                case double d:
                                    {
                                        _write.Add($"{settingName} = {node.ToString().Replace(",", ".")}");
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
                                                _write.Add($"{settingName} = [\"{ string.Join("\", \"", ta.Select(w => w.ToString())) }\"]");
                                            }
                                            else
                                            {
                                                _write.Add($"{settingName} = [{ string.Join(", ", ta.Select(w => w.ToString())) }]");
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

            File.WriteAllLines(_filePath, _write);
        }
    }

    public class ConfigSpec {
        [XmlElement("Setting")]
        public List<Setting> Settings { get; set; }
    }

    [XmlInclude(typeof(DropDown)), XmlInclude(typeof(Checkbox)), XmlInclude(typeof(TextEntry))]
    public abstract class Setting {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }

        public string DefaultValue { get; set; }

        public virtual void Load(System.Windows.Forms.Control container, Settings settings) {
            var lbl = new System.Windows.Forms.Label() { Text = Name };
            container.Controls.Add(lbl);
            lbl.Location = new System.Drawing.Point(10, 15);
            var ldesc = new System.Windows.Forms.Label() { Text = Description, AutoSize = true, MaximumSize = new System.Drawing.Size(200, 0) };
            container.Controls.Add(ldesc);
            ldesc.Location = new System.Drawing.Point(container.Width - 210, 10);
            ldesc.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        }

        public virtual void Save(Settings settings) {

        }
    }

    public class TextEntry : Setting {
        private System.Windows.Forms.TextBox _tb;

        public string Option { get; set; }

        [XmlElement("Suggest")]
        public List<string> Suggestions { get; set; }

        public override void Load(System.Windows.Forms.Control container, Settings settings) {
            base.Load(container, settings);
            _tb = new System.Windows.Forms.TextBox() { Text = settings.Get(Option) };
            _tb.Location = new System.Drawing.Point(150, 10);
            _tb.Width = 150;
            if (Suggestions.Any()) {
                _tb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
                _tb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                _tb.AutoCompleteCustomSource = new System.Windows.Forms.AutoCompleteStringCollection();
                _tb.AutoCompleteCustomSource.AddRange(Suggestions.ToArray());
            }
            container.Controls.Add(_tb);
        }

        public override void Save(Settings settings) {
            base.Save(settings);
            settings.Apply(Option + "=" + _tb.Text);
        }
    }

    public class DDOption {
        public string Text { get; set; }
        public string Settings { get; set; }

        public override string ToString() {
            return Text;
        }
    }
    public class DropDown : Setting {
        private System.Windows.Forms.ComboBox _cb;

        [XmlElement("Option")]
        public List<DDOption> Options { get; set; }

        public override void Load(System.Windows.Forms.Control container, Settings settings) {
            base.Load(container, settings);
            _cb = new System.Windows.Forms.ComboBox() { DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            _cb.Width = 150;
            _cb.Name = Name;
            foreach (var ddo in Options) {
                _cb.Items.Add(ddo);
                if (settings.IsMatched(ddo.Settings))
                    _cb.SelectedIndex = _cb.Items.Count - 1;
            }
            container.Controls.Add(_cb);
            _cb.Location = new System.Drawing.Point(150, 10);
        }

        public override void Save(Settings settings) {
            base.Save(settings);
            if (_cb.SelectedIndex >= 0) {
                DDOption ddo = _cb.SelectedItem as DDOption;
                settings.Apply(ddo.Settings);
            }
        }
    }

    public class Checkbox : Setting {
        private System.Windows.Forms.CheckBox _cb;

        public string TrueSetting { get; set; }
        public string FalseSetting { get; set; }

        public Checkbox() {
        }

        public override void Load(System.Windows.Forms.Control container, Settings settings) {
            base.Load(container, settings);
            container.Controls.RemoveAt(0); //don't want label
            _cb = new System.Windows.Forms.CheckBox() { Text = Name };
            _cb.Checked = settings.IsMatched(TrueSetting);
            _cb.Location = new System.Drawing.Point(150, 15);
            container.Controls.Add(_cb);
        }

        public override void Save(Settings settings) {
            base.Save(settings);
            settings.Apply(_cb.Checked ? TrueSetting : FalseSetting);
        }
    }
}
