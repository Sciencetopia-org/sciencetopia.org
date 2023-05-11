using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "此字段是必填项。")]
    [Display(Name = "用户名")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "此字段是必填项。")]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; }

    [Display(Name = "记住我?")]
    public bool RememberMe { get; set; }
}
