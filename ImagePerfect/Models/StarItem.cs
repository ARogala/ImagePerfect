using ReactiveUI;

namespace ImagePerfect.Models
{
    public class StarItem : ReactiveObject
    {
        public int Number { get; }
        private bool _isFilled;
        public bool IsFilled
        {
            get => _isFilled;
            set => this.RaiseAndSetIfChanged(ref _isFilled, value);
        }
        public StarItem(int number)
        {
            Number = number;
        }
    }
}
