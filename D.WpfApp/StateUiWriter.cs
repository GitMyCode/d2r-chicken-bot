using D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Controls;

namespace D.WpfApp {
    internal class StateUiWriter : IStateUiWriter {

        private readonly TaskScheduler _uiScheduler;
        public readonly TextPath m_Status;
        public readonly TextPath m_AdditionalData;


        public StateUiWriter(TextPath statusBox, TextPath additionalData, TaskScheduler uiScheduler) {
            _uiScheduler = uiScheduler;
            m_Status = statusBox;
            m_AdditionalData = additionalData;
        }

        public void WriteState(string state) {
            Task.Factory.StartNew(
                   () => {
                       m_Status.Text = state;
                   },
                   CancellationToken.None,
                   TaskCreationOptions.None,
                   this._uiScheduler);
        }

        public void WriteAdditionalData(string data) {
            Task.Factory.StartNew(
                   () => {
                       m_AdditionalData.Text = data;
                   },
                   CancellationToken.None,
                   TaskCreationOptions.None,
                   this._uiScheduler);
        }

        public void Clear() {
            WriteState("");
            WriteAdditionalData("");
        }
    }
}
