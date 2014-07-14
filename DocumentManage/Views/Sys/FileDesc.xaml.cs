using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class FileDesc : Page
    {
        private readonly FileDescDomainContext descContext = new FileDescDomainContext();
        private readonly EnumToStringValueConverter enumConverter = new EnumToStringValueConverter();
        public FileDesc()
        {
            Resources.Add("FileTypeNames", EnumHelper.GetNames(typeof(DocumentType)));
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FileTypeCombBox.SelectedIndex = 0;
        }

        private void OnFileTypeCombBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileTypeCombBox.SelectedItem != null)
            {
                var fileType = (DocumentType)EnumHelper.GetValue(typeof(DocumentType), FileTypeCombBox.SelectedItem.ToString());
                descContext.GetDescTemplateByType(fileType, (obj) =>
                    {
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            FileDescTemplateGrid.ItemsSource = obj.Value;
                        }
                    }, null);
            }
        }

        private void OnFormAutoGenerateField(object sender, DataFormAutoGeneratingFieldEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                var combbox = new ComboBox();
                combbox.ItemsSource = EnumHelper.GetNames(e.PropertyType);
                combbox.SelectedIndex = 0;
                combbox.SetBinding(Selector.SelectedItemProperty, new Binding
                {
                    Path = new PropertyPath(e.PropertyName),
                    Mode = BindingMode.TwoWay,
                    Converter = enumConverter,
                    ValidatesOnDataErrors = true
                });
                e.Field.IsReadOnly = e.PropertyName.Equals("DocType", StringComparison.CurrentCulture);
                e.Field.Content = combbox;
            }
        }

        private void OnFileDescGridAutoGenerateColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = enumConverter }
                };
            }
        }

        private void OnDescItemDeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var template = FormEditFileDesc.CurrentItem as FileDescTemplate;
            if (template != null)
            {
                template.Status = ActiveStatus.Deleted;
                descContext.UpdateTemplate(template);
            }
            e.Cancel = true;
        }

        private void OnFormCurrentItemChanged(object sender, EventArgs e)
        {
            var item = FormEditFileDesc.CurrentItem as FileDescTemplate;
            if (item != null)
            {
                var type = (DocumentType)EnumHelper.GetValue(typeof(DocumentType), FileTypeCombBox.SelectedItem.ToString());
                item.DocType = type;
            }
        }

        private void OnFormItemEditEnding(object sender, DataFormEditEndingEventArgs e)
        {
            if (e.EditAction == DataFormEditAction.Commit)
            {
                var template = FormEditFileDesc.CurrentItem as FileDescTemplate;
                if (template != null)
                {
                    switch (FormEditFileDesc.Mode)
                    {
                        case DataFormMode.AddNew:
                            descContext.CreateTemplate(template);
                            break;
                        case DataFormMode.Edit:
                            descContext.UpdateTemplate(template);
                            break;
                    }
                }
            }
        }

    }
}
