using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using FileHelpers;
using System.Windows.Forms;
using KT_TTAPI;
using System.Configuration;
using Utils.SmartAsyncMsgBox;

namespace MonkeyCancel
{
    class Manager : IDisposable
    {
        private readonly TTConnection m_ttConnection;
        private readonly MonkeyCancelFrm gui;
        private readonly Dictionary<ITTInstrumentState, Instrument> m_instMap = new Dictionary<ITTInstrumentState, Instrument>();
        private readonly InstrumentFactory m_instrumentFactory = new InstrumentFactory();

        public Manager(MonkeyCancelFrm gui)
        {
            this.gui = gui;
            gui.Disposed += Gui_Disposed;

            TTConnection.TTConnectionConfigurator config = TTConnection.TTConnectionConfigurator.newConfig()
                .TTUsername(ConfigurationManager.AppSettings["TTUSER"])
                .TTPassword(ConfigurationManager.AppSettings["TTPASSWORD"])
                .PriceGateway(ConfigurationManager.AppSettings["PriceGateway"])
                .OrderGateway(ConfigurationManager.AppSettings["OrderGateway"])
                .Exchange(ConfigurationManager.AppSettings["Exchange"])
                .UseTTImpliedEngine(bool.Parse(ConfigurationManager.AppSettings["UseTTImpliedEngine"]))
                .InstrumentThreadPoolSize(int.Parse(ConfigurationManager.AppSettings["InstrumentThreadPoolSize"]));

            m_ttConnection = config.build();
            m_ttConnection.OnInstrumentFound += m_ttConnection_OnInstrumentFound;
            m_ttConnection.OnAuthenticationStatusChange += m_ttConnection_OnAuthenticationStatusChange;
            m_ttConnection.OnFeedStatusChanged += m_ttConnection_OnFeedStatusChanged;
            m_ttConnection.Start();
        }

        private void Gui_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        void m_ttConnection_OnFeedStatusChanged(object sender, EventArgs e)
        {
            gui.UpdateGatewayStatus(TTConnection.GatewayType.Price, m_ttConnection.PriceFeedUp);
            gui.UpdateGatewayStatus(TTConnection.GatewayType.Order, m_ttConnection.OrderFeedUp);
            gui.UpdateGatewayStatus(TTConnection.GatewayType.Fill, m_ttConnection.FillFeedUp);
        }

        void m_ttConnection_OnAuthenticationStatusChange(object sender, AuthenticationChangeEventArgs e)
        {
            if (e.Successful)
                LoadInstruments();
            else
                SmartAsyncMsgBox.Show(e.StatusMsg, "Authentication Status");
        }

        void m_ttConnection_OnInstrumentFound(object sender, InstFoundEventArgs e)
        {
            //The instrumentState is initialized. Call the UpdateInstrument in order to finish the initialization of Instrument class.
            m_instrumentFactory.UpdateInstrument(e.instrumentState);

            gui.BeginInvoke(new Action(() => 
            {
                gui.AddInstrument(m_instMap[e.instrumentState]);
            }));
        }


        void LoadInstruments()
        {
            FileHelperEngine engine = new FileHelperEngine(typeof(InstrDef));

            string instFilePath = "config/instFiles/" + ConfigurationManager.AppSettings["InstrumentsFile"];
            InstrDef[] insts = engine.ReadFile(instFilePath) as InstrDef[];

            gui.BeginInvoke(new Action(() =>
            {
                //TTPROJECTS-1
                gui.Text = "MonkeyCancel - " + ConfigurationManager.AppSettings["configFilename"] + " " + instFilePath;
                gui.InstrumentDefinitions = insts;
            }));

            MessageBox.Show(insts.Length + " instruments in config");

            foreach (InstrDef instrDef in insts)
            {
                AddInstrument(instrDef);
            }
        }

        Instrument AddInstrument(InstrDef def)
        {
            ITTInstrumentState instState = m_ttConnection.AddInstrument(def.Exchange, def.ProdType, def.Product, def.Contract, def.Alias);

            Instrument new_instr = m_instrumentFactory.CreateInstrument(def, instState);

            m_instMap.Add(instState, new_instr);

            return new_instr;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_ttConnection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
