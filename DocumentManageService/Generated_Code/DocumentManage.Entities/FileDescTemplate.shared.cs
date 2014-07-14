namespace DocumentManage.Entities
{
    public partial class FileDescTemplate
    {
        public object Clone()
        {
            return new FileDescTemplate
                       {
                           TemplateId = TemplateId,
                           DocType = DocType,
                           Header = Header,
                           DataType = DataType,
                           MinimumValue = MinimumValue,
                           MaximumValue = MaximumValue,
                           ValueLength = ValueLength,
                           DefaultValue = DefaultValue,
                           IsRequired = IsRequired,
                           DisplayIndex = DisplayIndex,
                           Status = Status
                       };
        }
    }
}
