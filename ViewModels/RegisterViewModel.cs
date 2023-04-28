using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "用户名")]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "电子邮件")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "确认密码")]
    [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
    public string ConfirmPassword { get; set; }
}
