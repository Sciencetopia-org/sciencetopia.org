using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "此字段是必填项。")]
    [Display(Name = "用户名")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "此字段是必填项。")]
    [EmailAddress]
    [Display(Name = "电子邮件")]
    public string Email { get; set; }

    [Required(ErrorMessage = "此字段是必填项。")]
    [StringLength(100, ErrorMessage = "密码至少要有{2}个字符长，并且不能超过{1}个字符。", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "确认密码")]
    [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
    public string ConfirmPassword { get; set; }
}