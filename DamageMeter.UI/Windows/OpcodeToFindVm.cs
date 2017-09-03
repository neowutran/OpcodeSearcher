using System.ComponentModel;
using System.Runtime.CompilerServices;
using DamageMeter.UI.Annotations;

namespace DamageMeter.UI.Windows
{
    public class OpcodeToFindVm : INotifyPropertyChanged
    {
        public string OpcodeName { get; }

        private uint _opcode;
        public uint Opcode
        {
            get { return _opcode; }
            set
            {
                if (_opcode == value) return;
                _opcode = value;
                OnPropertyChanged(nameof(Opcode));
            }
        }

        public OpcodeToFindVm(string s, uint i)
        {
            Opcode = i;
            OpcodeName = s;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}