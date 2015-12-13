using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleNotes.Views
{
    /// <summary>
    /// Interaction logic for NoteEditView.xaml
    /// </summary>
    public partial class NoteEditView : UserControl, IViewFor<NoteEditViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(NoteEditViewModel), typeof(NoteEditView), new PropertyMetadata(default(NoteEditViewModel)));

        public NoteEditView()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (NoteEditViewModel)value; }
        }

        public NoteEditViewModel ViewModel
        {
            get { return (NoteEditViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
