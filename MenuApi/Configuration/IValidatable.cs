using System.ComponentModel.DataAnnotations;

namespace MenuApi.Configuration;

public interface IValidatable
{
    void Validate()
    {
        Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
    }
}
