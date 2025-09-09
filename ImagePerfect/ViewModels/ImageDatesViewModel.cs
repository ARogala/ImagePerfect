using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ImagePerfect.ViewModels
{
	public class ImageDatesViewModel : ViewModelBase
	{
        private ObservableCollection<int> _years;
        public ObservableCollection<int> Years
        {
            get => _years;
            set => this.RaiseAndSetIfChanged(ref _years, value);
        }

        private ObservableCollection<string> _yearMonths;
        public ObservableCollection<string> YearMonths
        {
            get => _yearMonths;
            set => this.RaiseAndSetIfChanged(ref _yearMonths, value);
        }

        private DateTimeOffset? _startDate;
        public DateTimeOffset? StartDate
        {
            get => _startDate;
            set => this.RaiseAndSetIfChanged(ref _startDate, value);
        }

        private DateTimeOffset? _endDate;
        public DateTimeOffset? EndDate
        {
            get => _endDate;
            set => this.RaiseAndSetIfChanged(ref _endDate, value);
        }
    }
}