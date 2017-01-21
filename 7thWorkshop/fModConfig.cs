/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iros._7th.Workshop {
    public partial class fModConfig : Form {

        private string _fileName;
        private string _iroPath;
        private string _audioPath;
        private System.IO.Stream _audioFile;
        private _7thWrapperLib.ModInfo _info;
        private Func<string, System.Drawing.Bitmap> _imageReader;
        private Func<string, System.IO.Stream> _audioReader;
        private Dictionary<string, int> _values;
        private _7thWrapperLib.ConfigOption _opt;
        private List<fLibrary.Constraint> _constraints;
        private NAudio.Wave.IWavePlayer _audio;

        public List<ProfileSetting> GetSettings() {
            return _values.Select(kv => new ProfileSetting() { ID = kv.Key, Value = kv.Value }).ToList();
        }

        public fModConfig() {
            InitializeComponent();
        }

        internal void Init(_7thWrapperLib.ModInfo info, Func<string, System.Drawing.Bitmap> imageReader, Func<string, System.IO.Stream> audioReader, ProfileItem profile, IEnumerable<fLibrary.Constraint> constraints, string iroPath) {
            _iroPath = iroPath;
            _info = info;
            _imageReader = imageReader;
            _audioReader = audioReader;
            _constraints = constraints.ToList();
            _values = profile.Settings.ToDictionary(s => s.ID, s => s.Value, StringComparer.InvariantCultureIgnoreCase);
            lbOptions.Items.AddRange(info.Options.ToArray());
            lbOptions.SelectedIndex = 0;
        }

        private void lbOptions_SelectedIndexChanged(object sender, EventArgs e) {
            _opt = lbOptions.SelectedItem as _7thWrapperLib.ConfigOption;
            if (_opt == null) {
                pOption.Enabled = false;
                return;
            }
            pOption.Enabled = true;
            lDescription.Text = _opt.Description;
            int value;
            if (!_values.TryGetValue(_opt.ID, out value)) value = _opt.Default;

            fLibrary.Constraint ct = _constraints.Find(c => c.Setting.Equals(_opt.ID, StringComparison.InvariantCultureIgnoreCase));

            switch (_opt.Type) {
                case _7thWrapperLib.OptionType.Bool:
                    if (ct.Require.Any()) {
                        value = ct.Require[0];
                        lCompat.Text = String.Format("This option cannot be changed due to compatibility with other mods ({0})", String.Join(", ", ct.ParticipatingMods));
                        cbOption.Enabled = false;
                    } else if (ct.Forbid.Any()) {
                        value = new[] { 0, 1 }.Except(ct.Forbid).FirstOrDefault();
                        lCompat.Text = String.Format("This option cannot be changed due to compatibility with other mods ({0})", String.Join(", ", ct.ParticipatingMods));
                        cbOption.Enabled = false;
                    } else {
                        lCompat.Text = String.Empty;
                        cbOption.Enabled = true;
                    }

                    cbOption.Visible = true;
                    ddOption.Visible = false;
                    cbOption.Text = _opt.Name;
                    cbOption.Checked = (value == 1);
                    cbOption_CheckedChanged(sender, e);
                    break;
                case _7thWrapperLib.OptionType.List:
                    IEnumerable<_7thWrapperLib.OptionValue> values = _opt.Values;
                    if (ct.Require.Any()) {
                        value = ct.Require[0];
                        lCompat.Text = String.Format("This option cannot be changed due to compatibility with other mods ({0})", String.Join(", ", ct.ParticipatingMods));
                        ddOption.Enabled = false;
                    } else if (ct.Forbid.Any()) {
                        var remove = values.Where(v => ct.Forbid.Contains(v.Value));
                        lCompat.Text = String.Format("The following values: {0} have been removed due to compatibility with other mods ({1})", String.Join(", ", remove.Select(o => o.Name)), String.Join(", ", ct.ParticipatingMods));
                        values = values.Except(remove);
                        if (!values.Any(v => v.Value == value)) value = values.First().Value;
                        ddOption.Enabled = true;
                    } else {
                        lCompat.Text = String.Empty;
                        ddOption.Enabled = true;
                    }

                    cbOption.Visible = false;
                    ddOption.Visible = true;
                    ddOption.Items.Clear();
                    ddOption.Items.AddRange(values.ToArray());
                    ddOption.SelectedIndex = ddOption.Items.IndexOf(_opt.Values.Find(o => o.Value == value));
                    break;
            }
        }


        private void StopAudio() {
            if (_audio != null) {
                _audio.Stop();
                _audio = null;
            }
        }

        private void SetupAudioPreview(_7thWrapperLib.OptionValue o) {
            bPreview.Visible = !String.IsNullOrWhiteSpace(o.PreviewAudio);
            bPreview.Tag = o.PreviewAudio;
            StopAudio();
        }

        private void cbOption_CheckedChanged(object sender, EventArgs e) {
            if (_opt == null) return;
            var o = _opt.Values.Find(v => v.Value == (cbOption.Checked ? 1 : 0));
            if (o != null)
                PB.Image = _imageReader(o.PreviewFile);
            else
                PB.Image = null;

            SetupAudioPreview(o);
            _values[_opt.ID] = cbOption.Checked ? 1 : 0;
        }

        private void ddOption_SelectedIndexChanged(object sender, EventArgs e) {
            if (_opt == null) return;
            var o = (_7thWrapperLib.OptionValue)ddOption.SelectedItem;
            if (o != null) {
                PB.Image = _imageReader(o.PreviewFile);
            }
            else
                PB.Image = null;
            SetupAudioPreview(o);

            if (o.PreviewAudio != "")
            {

                Console.WriteLine("AUDIO = " + o.PreviewAudio);
                _fileName = System.IO.Path.GetFileName(o.PreviewAudio);
                if (_iroPath.EndsWith(".iro"))
                {
                    string mod = _iroPath;
                    _audioPath = extractIros(mod, o.PreviewAudio);
                }
                else _audioFile = _audioReader(o.PreviewAudio);
            }
            _values[_opt.ID] = o.Value;
        }

        private void bPreview_Click(object sender, EventArgs e) {
            if (_audio == null) {
                _audio = new NAudio.Wave.WaveOut();

                if (_fileName.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (_audioPath != null) _audio.Init(new NAudio.Vorbis.VorbisWaveReader(_audioPath));
                    else if (_audioFile != null) _audio.Init(new NAudio.Vorbis.VorbisWaveReader(_audioFile));
                }
                else if (_fileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (_audioPath != null) _audio.Init(new NAudio.Wave.Mp3FileReader(_audioPath));
                    else if (_audioFile != null) _audio.Init(new NAudio.Wave.Mp3FileReader(_audioFile));
                }
                else return;
                _audio.Play();
            } else StopAudio();
        }

        private void fModConfig_FormClosed(object sender, FormClosedEventArgs e) {
            StopAudio();
        }

        private string extractIros(string iroFile, string path)
        {
            bPreview.Enabled = false;
            _7thWrapperLib.IrosArc iro = new _7thWrapperLib.IrosArc(iroFile);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string filter = path;
            string fn = "";
            foreach (string file in iro.AllFileNames())
            {
                if (!String.IsNullOrEmpty(filter) && (file.IndexOf(filter) < 0)) continue;
                byte[] data = iro.GetBytes(file);
                fn = System.IO.Path.Combine(System.IO.Path.GetTempPath(), file);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fn));
                System.IO.File.WriteAllBytes(fn, data);
            }
            sw.Stop();
            bPreview.Enabled = true;
            return fn;
        }
    }
}
