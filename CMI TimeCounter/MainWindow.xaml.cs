using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Linq;

namespace CMI_TimeCounter
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<CounterViewModel> counters;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            
            counters = new ObservableCollection<CounterViewModel>();
            counterList.ItemsSource = counters;
            
            // Timer für die Aktualisierung der Anzeige
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (var counter in counters)
            {
                counter.UpdateElapsedTime();
            }
        }

        private void BtnAddCounter_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CounterDialog();
            if (dialog.ShowDialog() == true)
            {
                counters.Add(new CounterViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = dialog.CounterTitle,
                    Description = dialog.CounterDescription
                });
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var counterId = (Guid)button.Tag;
            var counter = counters.FirstOrDefault(c => c.Id == counterId);
            if (counter != null)
            {
                counter.Start();
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var counterId = (Guid)button.Tag;
            var counter = counters.FirstOrDefault(c => c.Id == counterId);
            if (counter != null)
            {
                counter.Stop();
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var counterId = (Guid)button.Tag;
            var counter = counters.FirstOrDefault(c => c.Id == counterId);
            if (counter != null)
            {
                counter.Reset();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var counterId = (Guid)button.Tag;
            var counter = counters.FirstOrDefault(c => c.Id == counterId);
            if (counter != null)
            {
                counters.Remove(counter);
            }
        }
    }

    public class CounterViewModel : INotifyPropertyChanged
    {
        private DateTime? startTime;
        private TimeSpan elapsedTimeOffset;
        private bool isRunning;

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string ElapsedTime
        {
            get
            {
                var elapsed = GetElapsedTime();
                return elapsed.ToString(@"hh\:mm\:ss");
            }
        }

        public Visibility StartButtonVisibility => !isRunning ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StopButtonVisibility => isRunning ? Visibility.Visible : Visibility.Collapsed;

        public void Start()
        {
            if (!isRunning)
            {
                startTime = DateTime.Now;
                isRunning = true;
                OnPropertyChanged(nameof(StartButtonVisibility));
                OnPropertyChanged(nameof(StopButtonVisibility));
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                elapsedTimeOffset = GetElapsedTime();
                startTime = null;
                isRunning = false;
                OnPropertyChanged(nameof(StartButtonVisibility));
                OnPropertyChanged(nameof(StopButtonVisibility));
            }
        }

        public void Reset()
        {
            startTime = null;
            elapsedTimeOffset = TimeSpan.Zero;
            isRunning = false;
            OnPropertyChanged(nameof(ElapsedTime));
            OnPropertyChanged(nameof(StartButtonVisibility));
            OnPropertyChanged(nameof(StopButtonVisibility));
        }

        public void UpdateElapsedTime()
        {
            if (isRunning)
            {
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }

        private TimeSpan GetElapsedTime()
        {
            if (startTime.HasValue)
            {
                return elapsedTimeOffset + (DateTime.Now - startTime.Value);
            }
            return elapsedTimeOffset;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CounterDialog : Window
    {
        private System.Windows.Controls.TextBox titleTextBox;
        private System.Windows.Controls.TextBox descriptionTextBox;

        public string CounterTitle => titleTextBox.Text;
        public string CounterDescription => descriptionTextBox.Text;

        public CounterDialog()
        {
            Title = "Neuer Counter";
            Width = 400;
            Height = 250;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(50) });

            var inputPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(10) };
            
            inputPanel.Children.Add(new System.Windows.Controls.Label { Content = "Titel:" });
            titleTextBox = new System.Windows.Controls.TextBox { Margin = new Thickness(0, 0, 0, 10) };
            inputPanel.Children.Add(titleTextBox);

            inputPanel.Children.Add(new System.Windows.Controls.Label { Content = "Beschreibung:" });
            descriptionTextBox = new System.Windows.Controls.TextBox { 
                Height = 60,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 10)
            };
            inputPanel.Children.Add(descriptionTextBox);

            grid.Children.Add(inputPanel);

            var buttonPanel = new System.Windows.Controls.StackPanel { 
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10),
            };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 1);

            var okButton = new System.Windows.Controls.Button { 
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; };
            
            var cancelButton = new System.Windows.Controls.Button { 
                Content = "Abbrechen",
                Width = 80,
                IsCancel = true
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
}