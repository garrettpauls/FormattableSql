using ReactiveUI;
using System.Windows;
using System.Windows.Controls;

namespace SimpleNotes.Views
{
    public partial class NoteListView : UserControl, IViewFor<NoteListViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(NoteListViewModel), typeof(NoteListView), new PropertyMetadata(default(NoteListViewModel)));

        public NoteListView()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (NoteListViewModel)value; }
        }

        public NoteListViewModel ViewModel
        {
            get { return (NoteListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
