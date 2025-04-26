using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NaraChat.Contract.Models.Auth
{
    public sealed class RegisterUserDto
    {
        [Required(ErrorMessage = "لطفاً نام را وارد کنید.")]
        [StringLength(50, ErrorMessage = "نام نباید بیشتر از 50 کاراکتر باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "لطفاً نام خانوادگی را وارد کنید.")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نباید بیشتر از 50 کاراکتر باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "لطفاً نام کاربری را وارد کنید.")]
        [StringLength(50, ErrorMessage = "نام کاربری نباید بیشتر از 50 کاراکتر باشد.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "لطفاً رمز عبور را وارد کنید.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید بین 6 تا 100 کاراکتر باشد.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "لطفاً سن را وارد کنید.")]
        [Range(1, 120, ErrorMessage = "سن باید بین 1 تا 120 باشد.")]
        [Display(Name = "سن")]
        public int Age { get; set; }

        [Required(ErrorMessage = "لطفاً جنسیت را انتخاب کنید.")]
        [Display(Name = "جنسیت")]
        public bool Gender { get; set; }

        [Required(ErrorMessage = "لطفاً شهر را وارد کنید.")]
        [StringLength(100, ErrorMessage = "نام شهر نباید بیشتر از 100 کاراکتر باشد.")]
        [Display(Name = "شهر")]
        public string City { get; set; } = null!;

    }
}
