using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiclose {
    internal class Closer {

        private Process? _process = null;

        public Closer(string processName) {
            ProcessName = processName;
            Poll();
        }

        public string ProcessName { get; }
        public bool HasProcess => _process != null;

        public void Poll () {

            var ps = Process.GetProcessesByName(ProcessName);

            if (ps.Length == 0) {
                Debug.WriteLine($"\n\n\nNo processed named {ProcessName} found");
                _process = null;
                return;
            }

            if (ps.Length > 1) {
                Debug.WriteLine($"\n\n\nMany processes named {ProcessName} found");
                _process = null;
                return;
            }

            _process = ps[0];

        }

        public void Close() {
            if (!HasProcess) throw new InvalidOperationException();

            _process?.Kill();

        }

    }
}
