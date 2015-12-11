using KT_TTAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonkeyCancel
{
    public partial class MonkeyCancelFrm : Form
    {
        private readonly Size smallTextboxSize = new Size(35, 20);
        private readonly Size checkboxMinSize = new Size(160, 0);
        private readonly Size priceLabelSize = new Size(75, 20);
        private readonly Size priceEdgeLabelSize = new Size(35, 20);
        private readonly Color textBoxDirtyColor = Color.Yellow;
        private readonly Color textBoxCleanColor = Color.White;
        private readonly Color edgeLabelNegativeColor = Color.Yellow;

        Dictionary<CheckBox, Instrument> cxlCheckMap = new Dictionary<CheckBox, Instrument>();
        Dictionary<TextBox, Instrument> cxlEdgeTextboxMap = new Dictionary<TextBox, Instrument>();
        Dictionary<CheckBox, Instrument> minImpliedQtyCheckMap = new Dictionary<CheckBox, Instrument>();
        Dictionary<TextBox, Instrument> minImpliedQtyTextboxMap = new Dictionary<TextBox, Instrument>();

        //For prices
        Dictionary<IInstrument, Label> instrumentBidEdgeVwmptMap = new Dictionary<IInstrument, Label>();
        Dictionary<IInstrument, Label> instrumentVwmptMap = new Dictionary<IInstrument, Label>();
        Dictionary<IInstrument, Label> instrumentAdjustmentMap = new Dictionary<IInstrument, Label>();
        Dictionary<IInstrument, Label> instrumentAskEdgeVwmptMap = new Dictionary<IInstrument, Label>();
        object intrumentsUpdateLock = new object();

        public InstrDef[] InstrumentDefinitions { get; set; }

        List<Instrument> m_Instruments = new List<Instrument>(16);

        public MonkeyCancelFrm()
        {
            InitializeComponent();
        }

        public void UpdateGatewayStatus(TTConnection.GatewayType type, bool isUp)
        {
            ToolStripStatusLabel lbl = null;
            switch (type)
            {
                case TTConnection.GatewayType.Order:
                    lbl = toolStripStatusLabelOrderServer;
                    break;
                case TTConnection.GatewayType.Fill:
                    lbl = toolStripStatusLabelFillServer;
                    break;
                case TTConnection.GatewayType.Price:
                    lbl = toolStripStatusLabelPriceServer;
                    break;
            }

            if (lbl != null)
            {
                Color newColor = isUp ? Color.Green : Color.Red;
                BeginInvoke(new Action(() => { lbl.ForeColor = newColor; }));
            }
        }

        public void AddInstrument(Instrument i)
        {
            m_Instruments.Add(i);

            /*
             * edge cxl
            */
            TextBox bidEdgeCxlTextBox = buildNewShortTextbox("bidCxlEdgeTextbox" + i.getAlias());
            cxlEdgeTextboxMap.Add(bidEdgeCxlTextBox, i);

            TextBox askEdgeCxlTextBox = buildNewShortTextbox("askCxlEdgeTextbox" + i.getAlias());
            cxlEdgeTextboxMap.Add(askEdgeCxlTextBox, i);

            CheckBox bidCxlCheckBox = buildNewCheckbox("bidCxlCheckBox" + i.getAlias(), i.getAlias() + " Bid Cxl", bidCxl_CheckedChanged, true);
            cxlCheckMap.Add(bidCxlCheckBox, i);

            CheckBox askCxlCheckBox = buildNewCheckbox("askCxlCheckBox" + i.getAlias(), i.getAlias() + " Ask Cxl", askCxl_CheckedChanged, true);
            cxlCheckMap.Add(askCxlCheckBox, i);

            /*
             * implied cxl
            */
            TextBox minImpliedQtyCxlTextBox = buildNewShortTextbox("minImpliedQtyTextbox" + i.getAlias());
            minImpliedQtyTextboxMap.Add(minImpliedQtyCxlTextBox, i);

            CheckBox minImpliedCxlCheckBox = buildNewCheckbox("minImpliedCxlCheckBox" + i.getAlias(), i.getAlias() + " Implied Cxl", minImpliedQtyCxl_CheckedChanged, false);
            minImpliedQtyCheckMap.Add(minImpliedCxlCheckBox, i);


            /*
             * price
            */
            Label bidEdgeVwmptLabel = buildPriceLabel("bidEdgeVwmptLabel" + i.getAlias(), priceEdgeLabelSize);
            instrumentBidEdgeVwmptMap.Add(i, bidEdgeVwmptLabel);

            Label vwmptLabel = buildPriceLabel("vwmptLabel" + i.getAlias(), priceLabelSize);
            instrumentVwmptMap.Add(i, vwmptLabel);

            Label adjustmentLabel = buildPriceLabel("adjustmentLabel" + i.getAlias(), priceLabelSize);
            instrumentAdjustmentMap.Add(i, adjustmentLabel);

            Label askEdgeVwmptLabel = buildPriceLabel("askEdgeVwmptLabel" + i.getAlias(), priceEdgeLabelSize);
            instrumentAskEdgeVwmptMap.Add(i, askEdgeVwmptLabel);


            /*
             * build layout
            */

            //Get index of new panel
            int newPanelIndex = GetIndexOfInstrument(i.getAlias());

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.AutoSize = true;
            panel.Name = "panel" + String.Format("{0:00000}", newPanelIndex);
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.Margin = new Padding(0, 0, 0, 0);
            
            panel.Controls.Add(bidEdgeCxlTextBox);
            panel.Controls.Add(bidCxlCheckBox);
            panel.Controls.Add(askEdgeCxlTextBox);
            panel.Controls.Add(askCxlCheckBox);
            panel.Controls.Add(buildSpacerLabel()); //spacer
            panel.Controls.Add(minImpliedQtyCxlTextBox);
            panel.Controls.Add(minImpliedCxlCheckBox);
            panel.Controls.Add(bidEdgeVwmptLabel);
            panel.Controls.Add(vwmptLabel);
            panel.Controls.Add(adjustmentLabel);
            panel.Controls.Add(askEdgeVwmptLabel);

            flowLayoutPanelInstruments.Controls.Add(panel);

            int numberOfPanels = flowLayoutPanelInstruments.Controls.Count;
            if (numberOfPanels > 1)
            {
                int currentIndex = numberOfPanels - 1;
                while (currentIndex > 0)
                {
                    string currentName = flowLayoutPanelInstruments.Controls[currentIndex].Name;
                    string prevName = flowLayoutPanelInstruments.Controls[currentIndex - 1].Name;

                    if (currentName.CompareTo(prevName) > 0)
                    {
                        break; //The order is OK.
                    }

                    //Swap
                    flowLayoutPanelInstruments.Controls.SetChildIndex(flowLayoutPanelInstruments.Controls[currentIndex], currentIndex - 1);
                    currentIndex--;
                }
            }
        }


        private int GetIndexOfInstrument(string alias)
        {
            if (InstrumentDefinitions != null)
            {
                for (int index = 0; index < InstrumentDefinitions.Length; index++)
                {
                    if (InstrumentDefinitions[index].Alias == alias)
                    {
                        return index;
                    }
                }
            }

            return int.MaxValue;
        }

        private TextBox buildNewShortTextbox(String name)
        {
            TextBox tb = new TextBox();
            tb.Text = "0";
            tb.Name = name;
            tb.TextChanged += textBox_TextChanged;
            tb.Size = smallTextboxSize;
            return tb;
        }

        private Label buildPriceLabel(string name, Size size)
        {
            Label lbl = new Label();
            lbl.Name = name;
            lbl.Text = "NaN";
            lbl.AutoSize = false;
            lbl.Size = size;
            lbl.TextAlign = ContentAlignment.MiddleRight;
            return lbl;
        }

        private CheckBox buildNewCheckbox(string name, String text, EventHandler checkedChangedHandler, bool initialCheckedState)
        {
            CheckBox cb = new CheckBox();
            cb.Checked = initialCheckedState;
            cb.CheckedChanged += checkedChangedHandler;
            cb.Name = name;
            cb.Text = text;
            cb.AutoSize = true;
            cb.MinimumSize = checkboxMinSize;
            return cb;
        }

        private Label buildSpacerLabel()
        {
            Label lbl = new Label();
            lbl.Size = smallTextboxSize;
            return lbl;
        }

        void askCxl_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkbox = (CheckBox)sender;

            var instrument = cxlCheckMap[chkbox];
            instrument.EnableAskCxl = chkbox.Checked;
            instrument.UpdateIfModified();
        }

        void bidCxl_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkbox = (CheckBox)sender;

            var instrument = cxlCheckMap[chkbox];
            instrument.EnableBidCxl = chkbox.Checked;
            instrument.UpdateIfModified();
        }

        void minImpliedQtyCxl_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;

            var instrument = minImpliedQtyCheckMap[cb];
            instrument.EnableImpliedCxl = cb.Checked;
            instrument.UpdateIfModified();
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.BackColor = textBoxDirtyColor;
        }

        private void buttonSaveEdges_Click(object sender, EventArgs e)
        {
            try
            {
                //cxl edges
                foreach (KeyValuePair<TextBox, Instrument> cxlEdgeKvp in cxlEdgeTextboxMap)
                {
                    float cxlEdge = (float)Convert.ToDouble(cxlEdgeKvp.Key.Text);

                    if (cxlEdgeKvp.Key.Name.Contains("bid"))
                        cxlEdgeKvp.Value.BidCxlEdge = cxlEdge;
                    else
                        cxlEdgeKvp.Value.AskCxlEdge = cxlEdge;

                    cxlEdgeKvp.Key.BackColor = textBoxCleanColor;
                }

                //implied cxl min qty
                foreach (KeyValuePair<TextBox, Instrument> impliedCxlMinQtyKvp in minImpliedQtyTextboxMap)
                {
                    int minQty = Convert.ToInt32(impliedCxlMinQtyKvp.Key.Text);
                    impliedCxlMinQtyKvp.Value.MinImpliedQty = minQty;

                    impliedCxlMinQtyKvp.Key.BackColor = textBoxCleanColor;
                }

                foreach (KeyValuePair<TextBox, Instrument> cxlEdgeKvp in cxlEdgeTextboxMap)
                    cxlEdgeKvp.Value.UpdateIfModified();

                foreach (KeyValuePair<TextBox, Instrument> impliedCxlMinQtyKvp in minImpliedQtyTextboxMap)
                    impliedCxlMinQtyKvp.Value.UpdateIfModified();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonWriteDump_Click(object sender, EventArgs e)
        {
            var dumpWriter = new DumpWriter();

            foreach(var instrument in m_Instruments)
            {
                instrument.WriteDump(dumpWriter);
            }
        }


        /// <summary>
        /// This method updates the prices of all instruments by the timer (every 250 ms).
        /// The lock is used because the Dictionaries are modified on startup from other thread.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void pricesUpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (intrumentsUpdateLock)
            {
                foreach (var pair in instrumentBidEdgeVwmptMap)
                {
                    var edgeValue = pair.Key.BidEdgeVwmpt;
                    var label = pair.Value;
                    label.Text = edgeValue.ToString("F2");
                    label.BackColor = edgeValue < 0 ? edgeLabelNegativeColor : Color.Empty;
                }

                foreach (var pair in instrumentVwmptMap)
                    pair.Value.Text = pair.Key.Vwmpt.ToString("F7");

                foreach (var pair in instrumentAdjustmentMap)
                    pair.Value.Text = (pair.Key.AdjustedVwmpt - pair.Key.Vwmpt).ToString("F7");

                foreach (var pair in instrumentAskEdgeVwmptMap)
                {
                    var edgeValue = pair.Key.AskEdgeVwmpt;
                    var label = pair.Value;
                    label.Text = edgeValue.ToString("F2");
                    label.BackColor = edgeValue < 0 ? edgeLabelNegativeColor : Color.Empty;
                }
            }
        }
    }
}
