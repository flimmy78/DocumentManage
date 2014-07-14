namespace DocumentManage.Entities
{
	public partial class SystemUser
	{
        public object Clone()
        {
            return new SystemUser
            {
                UserId = UserId,
                UserCode = UserCode,
                UserName = UserName,
                UserPassword = UserPassword,
                Gender = Gender,
                RealName = RealName,
                Telphone = Telphone,
                Mobile = Mobile,
                Fax = Fax,
                QQ = QQ,
                Email = Email,
                Address = Address,
                Description = Description,
                Status = Status
            };
        }
    }
}
