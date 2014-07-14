using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using DocumentManage.Entities;
using DocumentManage.Views;
using DocumentManageService.Web;

namespace DocumentManage.Controls
{
    public partial class FileDescriber : UserControl
    {
        private EventHandler _fileDeletedEventHandler;

        public event EventHandler OnFileDeleted
        {
            add { _fileDeletedEventHandler += value; }
            remove { if (_fileDeletedEventHandler != null) _fileDeletedEventHandler -= value; }
        }

        public readonly static DependencyProperty FileInfoProperty = DependencyProperty.Register("FileInfo", typeof(Document), typeof(FileDescriber),
            new PropertyMetadata(null, OnFileInfoChanged));

        private static void OnFileInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FileDescriber)d).OnFileInfoChanged(e.NewValue as Document);
        }

        public Document FileInfo
        {
            get { return (Document)GetValue(FileInfoProperty); }
            set { SetValue(FileInfoProperty, value); }
        }

        public readonly static DependencyProperty ReadonlyProperty = DependencyProperty.Register("Readonly", typeof(bool), typeof(FileDescriber),
            new PropertyMetadata(false, OnReadonlyChanged));

        private static void OnReadonlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FileDescriber)d).OnReadonlyChanged(e.NewValue);
        }

        private void OnReadonlyChanged(object obj)
        {
            bool b = Convert.ToBoolean(obj);
            btnSave.Visibility = b ? Visibility.Collapsed : Visibility.Visible;
            txtFileName.IsReadOnly = b;
        }

        public bool Readonly
        {
            get { return (bool)GetValue(ReadonlyProperty); }
            set { SetValue(ReadonlyProperty, value); }
        }

        private void OnFileInfoChanged(Document doc)
        {
            if (doc == null || doc.FileType == DocumentType.Folder)
            {
                Visibility = Visibility.Collapsed;
                return;
            }
            Visibility = Visibility.Visible;
            txtFileName.Text = doc.FileName;

            new FileDescDomainContext().GetActiveTemplateByType(doc.FileType, InitFileDescControls, null);
        }

        public readonly static DependencyProperty DescriptionsProperty = DependencyProperty.Register("Descriptions", typeof(List<DocumentDesc>), typeof(FileDescriber),
            new PropertyMetadata(new List<DocumentDesc>()));

        public List<DocumentDesc> Descriptions
        {
            get { return GetValue(DescriptionsProperty) as List<DocumentDesc>; }
            set
            {
                SetValue(DescriptionsProperty, value);
                SetDocumentDescriptions(value);
            }
        }

        private void SetDocumentDescriptions(List<DocumentDesc> descs)
        {
            if (descs != null && DescribeListPanel.Children.Count > 0 && descs.Count > 0)
            {
                foreach (Grid grid in DescribeListPanel.Children)
                {
                    var desc = grid.DataContext as FileDescTemplate;
                    var ctrl = grid.Children[1] as FrameworkElement;
                    if (desc != null && ctrl != null)
                    {
                        var docDesc = descs.FirstOrDefault(o => o.TemplateId == desc.TemplateId);
                        if (docDesc == null)
                            continue;
                        ctrl.Tag = docDesc.Id;
                        string strValue = docDesc.Description;
                        if (Readonly)
                        {
                            ((Label)ctrl).Content = strValue;
                        }
                        else
                        {
                            DateTime dt;
                            switch (ctrl.GetType().Name)
                            {
                                case "DatePicker":
                                    if (DateTime.TryParse(strValue, out dt))
                                        ((DatePicker)ctrl).SelectedDate = dt;
                                    break;
                                case "NumericUpDown":
                                    double d;
                                    if (double.TryParse(strValue, out d))
                                        ((NumericUpDown)ctrl).Value = d;
                                    break;
                                case "TextBox":
                                    ((TextBox)ctrl).Text = strValue;
                                    break;
                                case "TimePicker":
                                    if (DateTime.TryParse(strValue, out dt))
                                        ((TimePicker)ctrl).Value = dt;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public List<DocumentDesc> GetDocumentDescriptions()
        {
            var list = new List<DocumentDesc>();
            foreach (Grid grid in DescribeListPanel.Children)
            {
                var template = grid.DataContext as FileDescTemplate;
                if (grid.Children.Count > 1 && template != null)
                {
                    var ctrl = grid.Children[1] as FrameworkElement;
                    if (ctrl == null)
                        continue;
                    string strValue = string.Empty;
                    switch (ctrl.GetType().Name)
                    {
                        case "DatePicker":
                            var dp = ctrl as DatePicker;
                            if (dp != null && dp.SelectedDate.HasValue)
                                strValue = dp.SelectedDate.Value.ToShortDateString();
                            break;
                        case "NumericUpDown":
                            var nu = ctrl as NumericUpDown;
                            if (nu != null)
                                strValue = nu.Value.ToString();
                            break;
                        case "TextBox":
                            strValue = ((TextBox)ctrl).Text;
                            break;
                        case "TimePicker":
                            var tp = ctrl as TimePicker;
                            if (tp != null && tp.Value.HasValue)
                                strValue = tp.Value.Value.ToLongTimeString();
                            break;
                    }
                    var descItem = new DocumentDesc
                                       {
                                           Identity = ctrl.Tag != null ? Convert.ToInt32(ctrl.Tag) : 0,
                                           DocumentId = FileInfo.DocumentId,
                                           TemplateId = template.TemplateId,
                                           Header = template.Header,
                                           TemplateInfo = template.Clone() as FileDescTemplate,
                                           Description = strValue,
                                       };
                    list.Add(descItem);
                }
            }
            FileInfo.Descriptions = list;
            return list;
        }

        private void InitFileDescControls(InvokeOperation<List<FileDescTemplate>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                DescribeListPanel.Children.Clear();
                if (obj.Value.Count > 0)
                {
                    DescribeListPanel.Visibility = Visibility.Visible;
                    foreach (var item in obj.Value)
                    {
                        var grid = new Grid { Height = 30, DataContext = item };
                        grid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 100, Width = GridLength.Auto });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        DescribeListPanel.Children.Add(grid);
                        var label = new Label();
                        label.Content = item.Header;
                        label.Style = Application.Current.Resources["ItemTitleLabelStyle"] as Style;
                        grid.Children.Add(label);
                        if (Readonly)
                        {
                            var lblValue = new Label();
                            lblValue.Style = Application.Current.Resources["ItemTitleLabelStyle"] as Style;
                            lblValue.Content = string.Empty;
                            Grid.SetColumn(lblValue, 1);
                            grid.Children.Add(lblValue);
                        }
                        else
                        {
                            switch (item.DataType)
                            {
                                case CustomDataType.DateTime:
                                case CustomDataType.Date:
                                    var dp = new DatePicker
                                    {
                                        SelectedDate = DateTime.Now.Date,
                                        SelectedDateFormat = DatePickerFormat.Short
                                    };
                                    if (!string.IsNullOrEmpty(item.MinimumValue))
                                    {
                                        DateTime dt;
                                        if (DateTime.TryParse(item.MinimumValue, out dt))
                                            dp.DisplayDateStart = dt;
                                    }
                                    if (!string.IsNullOrEmpty(item.MaximumValue))
                                    {
                                        DateTime dt;
                                        if (DateTime.TryParse(item.MaximumValue, out dt))
                                            dp.DisplayDateEnd = dt;
                                    }
                                    if (!string.IsNullOrEmpty(item.DefaultValue))
                                    {
                                        DateTime dt;
                                        if (DateTime.TryParse(item.DefaultValue, out dt))
                                            dp.SelectedDate = dt;
                                    }
                                    Grid.SetColumn(dp, 1);
                                    grid.Children.Add(dp);
                                    break;
                                case CustomDataType.Time:
                                    var tp = new TimePicker
                                    {
                                        Format = new CustomTimeFormat("hh:mm:ss"),
                                        Value = DateTime.Now,
                                    };
                                    Grid.SetColumn(tp, 1);
                                    grid.Children.Add(tp);
                                    break;
                                case CustomDataType.Float:
                                case CustomDataType.Integer:
                                    var floatInput = new NumericUpDown
                                    {
                                        DecimalPlaces = item.DataType == CustomDataType.Float ? 6 : 0
                                    };
                                    if (!string.IsNullOrEmpty(item.MinimumValue))
                                    {
                                        double d;
                                        if (double.TryParse(item.MinimumValue, out d))
                                            floatInput.Minimum = d;
                                    }
                                    if (!string.IsNullOrEmpty(item.MaximumValue))
                                    {
                                        double d;
                                        if (double.TryParse(item.MaximumValue, out d))
                                            floatInput.Maximum = d;
                                    }
                                    if (!string.IsNullOrEmpty(item.DefaultValue))
                                    {
                                        double d;
                                        if (double.TryParse(item.DefaultValue, out d))
                                            floatInput.Value = d;
                                    }
                                    Grid.SetColumn(floatInput, 1);
                                    grid.Children.Add(floatInput);
                                    break;
                                default:
                                    var txtBox = new TextBox
                                    {
                                        MaxLength = item.ValueLength < 1 ? 255 : item.ValueLength,
                                        Text = item.DefaultValue
                                    };
                                    Grid.SetColumn(txtBox, 1);
                                    grid.Children.Add(txtBox);
                                    break;
                            }
                        }
                    }
                    SetDocumentDescriptions(FileInfo.Descriptions);
                }
                else
                    DescribeListPanel.Visibility = Visibility.Collapsed;
            }
        }

        public FileDescriber()
        {
            InitializeComponent();
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            if (FileInfo == null)
                return;
            if (CustomMessageBox.Ask(string.Format("您确定要删除文件[{0}]吗", FileInfo.FileName)) && _fileDeletedEventHandler != null)
            {
                _fileDeletedEventHandler(this, null);
            }
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (FileInfo == null)
                return;
            FileInfo.FileName = txtFileName.Text;
            Descriptions = GetDocumentDescriptions();
        }
    }
}
