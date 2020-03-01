﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crystallography;

namespace IPAnalyzer
{
    public partial class FormSequentialImage : Form
    {

        public bool MultiSelection
        {
            set
            {
                skipEvent = true;
                if (checkBoxMultiSelection.Checked != value)
                    checkBoxMultiSelection.Checked = value;
                checkBoxAverage.Enabled = checkBoxMultiSelection.Checked;
                listBox_SelectedIndexChanged(new object(), new EventArgs());
                
                listBox.SelectionMode = checkBoxMultiSelection.Checked ? SelectionMode.MultiExtended : SelectionMode.One;
                trackBar1.Enabled = !value;
                skipEvent = false;
            }
            get { return checkBoxMultiSelection.Checked; }
        }

        public bool AverageMode
        {
            set
            {
                if (checkBoxAverage.Checked != value)
                    checkBoxAverage.Checked = value;
            }
            get { return checkBoxAverage.Checked; }

        }

        public FormMain formMain;

        public bool SkipCalcFreq = false;

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            set
            {
                if (Ring.SequentialImageIntensities != null && Ring.SequentialImageIntensities.Count > value)
                {
                    selectedIndex = value;

                    skipEvent = true;
                    trackBar1.Value = selectedIndex;
                    if(listBox.SelectionMode!= SelectionMode.MultiExtended)
                        listBox.SelectedIndex = selectedIndex;
                    skipEvent = false;
                    
                    for (int i = 0; i < Ring.Intensity.Count; i++)
                            Ring.IntensityOriginal[i] = Ring.SequentialImageIntensities[selectedIndex][i];
                    
                    formMain.FlipRotate_Pollalization_Background(false);


                    if (!SkipCalcFreq)
                    {
                        Ring.CalcFreq();
                        formMain.SetFrequencyProfile();
                        formMain.SetStasticalInformation(false);
                    }
                    formMain.SetText(formMain.FileName, "#" + Ring.SequentialImageNames[selectedIndex]);

                    if (Ring.SequentialImageEnergy != null && Ring.SequentialImageEnergy.Count == Ring.SequentialImageIntensities.Count)//イメージごとにエネルギーが設定されているとき
                    {
                        formMain.FormProperty.WaveLength = UniversalConstants.Convert.EnergyToXrayWaveLength(Ring.SequentialImageEnergy[selectedIndex]);
                    }
                   
                    formMain.SetInformation();


                    formMain.Draw();
                }
            }
            get
            {
                if (Ring.SequentialImageIntensities == null || Ring.SequentialImageIntensities.Count < 2)
                    return -1;
                else
                    return selectedIndex;
            }
        }

        private int[] selectedIndices = new int[] { };
        public int[] SelectedIndices
        {
            set
            {
                skipEvent = true;
                int selectedIndex = listBox.SelectedIndex;
                for (int i = 0; i < listBox.Items.Count; i++)
                    listBox.SetSelected(i, value.Contains(i));
                if(value.Contains(selectedIndex))
                    listBox.SelectedIndex = selectedIndex;
                skipEvent = false;
                for (int i = 0; i < value.Length; i++)
                    if (Ring.SequentialImageIntensities == null || value[i] < 0 || value[i] >= Ring.Intensity.Count)
                        return;
                selectedIndices = value;

                if (AverageMode)
                {
                    checkBoxMultiSelection.Checked = true;
                    double energy = 0;
                    for (int j = 0; j < selectedIndices.Length; j++)
                    {
                        if (Ring.ImageType == Ring.ImageTypeEnum.HDF5)
                            energy += Ring.SequentialImageEnergy[selectedIndices[j]] / selectedIndices.Length;
                        for (int i = 0; i < Ring.Intensity.Count; i++)
                            if (j == 0)
                                Ring.IntensityOriginal[i] = Ring.SequentialImageIntensities[selectedIndices[j]][i] / selectedIndices.Length;
                            else
                                Ring.IntensityOriginal[i] += Ring.SequentialImageIntensities[selectedIndices[j]][i] / selectedIndices.Length;
                    }
                    //for (int i = 0; i < Ring.Intensity.Count; i++)
                    //    formMain.pseudoBitmap.SrcValuesGray[i] = formMain.pseudoBitmap.SrcValuesGrayOriginal[i] = Ring.Intensity[i];

                    formMain.FlipRotate_Pollalization_Background(false);

                    if (Ring.ImageType == Ring.ImageTypeEnum.HDF5)
                        formMain.FormProperty.WaveLength = UniversalConstants.Convert.EnergyToXrayWaveLength(energy);

                    Ring.CalcFreq();
                    //formMain.SetFrequencyProfile();

                    string text = "";

                   
                    for (int i = 0; i < selectedIndices.Length; i++)
                    {
                        if (i == 0)
                                text += Ring.SequentialImageNames[selectedIndices[i]];
                        else if (selectedIndices[i] == selectedIndices[i - 1] + 1)
                        {
                            if (!text.EndsWith("-"))
                                text += "-";
                            if (i == selectedIndices.Length - 1)
                                text += Ring.SequentialImageNames[selectedIndices[i]];
                        }
                        else
                        {
                            if (text.EndsWith("-"))
                                text +=Ring.SequentialImageNames[selectedIndices[i-1]] ;
                            text += ", " + Ring.SequentialImageNames[selectedIndices[i]];
                        }
                    }
                    formMain.SetText(formMain.FileName, "Ave. of #" + text);
                    formMain.SetStasticalInformation(false);
                    formMain.SetFrequencyProfile();
                    formMain.Draw();
                }
                else
                    SelectedIndex = listBox.SelectedIndex;

            }
            get { return selectedIndices; }
        }


        public int MaximumNumber
        {
            set
            {
                trackBar1.Maximum = value - 1;
                if (value > 20)
                    trackBar1.LargeChange = value / 10;
                else if (value > 3)
                    trackBar1.LargeChange = 2;

                listBox.Items.Clear();
                for (int i = 0; i < value; i++)
                    listBox.Items.Add(formMain.FileName + "  #" + Ring.SequentialImageNames[i]);

            }
            get { return trackBar1.Maximum+1; }
        }


        public FormSequentialImage()
        {
            InitializeComponent();
        }

        private void FormSequentialImage_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            formMain.toolStripButtonImageSequence.Checked = false;
        }

        bool skipEvent = false;

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (skipEvent) return;

            if (!MultiSelection && SelectedIndex != trackBar1.Value)
            {
                skipEvent = true;
                SelectedIndex = trackBar1.Value;
                skipEvent = false;
            }
        }

        private void checkBoxMultiSelection_CheckedChanged(object sender, EventArgs e)
        {
            if (skipEvent) return;
            MultiSelection = checkBoxMultiSelection.Checked;
            
        }

        private void checkBoxAverage_CheckedChanged(object sender, EventArgs e)
        {
            listBox_SelectedIndexChanged(new object(), new EventArgs());
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (skipEvent) return;

            skipEvent = true;
            int[] temp = new int[listBox.SelectedIndices.Count];
            for (int i = 0; i < listBox.SelectedIndices.Count; i++)
                temp[i] = listBox.SelectedIndices[i];
            SelectedIndices = temp;

            skipEvent = false;
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            //背景を描画する
            //項目が選択されている時は強調表示される
            e.DrawBackground();

            //ListBoxが空のときにListBoxが選択されるとe.Indexが-1になる
            if (e.Index > -1)
            {
                //描画する文字列の取得
                string txt = ((ListBox)sender).Items[e.Index].ToString();
                string num = "";
                if (txt.Contains(".h5") || Ring.SequentialImageEnergy!=null)
                    num = e.Index.ToString("000") + ":  ";

                //文字列の描画

                Font numFont = new System.Drawing.Font(listBox.Font.FontFamily, listBox.Font.Size, FontStyle.Italic);
                float numWidth = e.Graphics.MeasureString(num, numFont).Width;

                Color c = ((e.State & DrawItemState.Selected) != DrawItemState.Selected) ? Color.Blue : Color.LightGreen ;

                e.Graphics.DrawString(num, numFont, new SolidBrush(c), new RectangleF(e.Bounds.X, e.Bounds.Y, numWidth, e.Bounds.Height));
                e.Graphics.DrawString(txt, listBox.Font, new SolidBrush(e.ForeColor), new RectangleF(e.Bounds.X + numWidth, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                //後始末
            }

            //フォーカスを示す四角形を描画
            e.DrawFocusRectangle();
        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (checkBoxMultiSelection.Checked && e.Control && e.KeyCode == Keys.A)
            {
                skipEvent = true;
                for (int i = 0; i < listBox.Items.Count; i++)
                    listBox.SetSelected(i, true);
                skipEvent = false;
                listBox_SelectedIndexChanged(selectedIndex, new EventArgs());
            }
        }

        private void RadioButtonGetProfileOnlyTopmost_CheckedChanged(object sender, EventArgs e)
        {
            if (skipEvent) return;

            skipEvent = true;

            if (radioButtonGetProfileAllImages.Checked)
            {
                formMain.toolStripMenuItemAllSequentialImages.Checked = true;
                formMain.toolStripMenuItemSelectedSequentialImages.Checked = false;
            }
            else if (radioButtonGetProfileOnlyTopmost.Checked)
            {
                formMain.toolStripMenuItemAllSequentialImages.Checked = false;
                formMain.toolStripMenuItemSelectedSequentialImages.Checked = false;
            }
            else if(radioButtonGetProfileSelectedImages.Checked)
            {
                formMain.toolStripMenuItemAllSequentialImages.Checked = false;
                formMain.toolStripMenuItemSelectedSequentialImages.Checked = true;
            }
            skipEvent = false;
        }

        internal void setRadio()
        {
            if (skipEvent) return;
            skipEvent = true;
            if (formMain.toolStripMenuItemAllSequentialImages.Checked)
                radioButtonGetProfileAllImages.Checked = true;
            else if (formMain.toolStripMenuItemSelectedSequentialImages.Checked)
                radioButtonGetProfileSelectedImages.Checked = true;
            else
                radioButtonGetProfileOnlyTopmost.Checked = true;
            skipEvent = false;
        }
    }
}