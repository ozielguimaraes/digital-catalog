namespace MeuCatalogo.Infrastructure.Validation;

// public class ValidatableObject<T>
//     {
//         private readonly List<IValidationRule<T>> _validations;
// 		private List<string> _errors;
//         private T _value;
//         private bool _isValid;
//         private string _placeholder;
//         private bool _IsVisible;
//
//         public bool IsVisible
//         {
//             get => _IsVisible;
//             set
//             {
//                 _IsVisible = value;
//                 RaisePropertyChanged(() => IsVisible);
//             }
//         }
//
//
//         public string Placeholder
//         {
//             get { return _placeholder; }
//             set
//             {
//                 _placeholder = value;
//                 RaisePropertyChanged(() => Placeholder);
//             }
//         }
//
//
//         public List<IValidationRule<T>> Validations => _validations;
//
// 		public List<string> Errors
// 		{
// 			get
// 			{
// 				return _errors;
// 			}
// 			set
// 			{
// 				_errors = value;
// 				RaisePropertyChanged(() => Errors);
// 			}
// 		}
//
//         public T Value
//         {
//             get
//             {
//                 return _value;
//             }
//             set
//             {
//                 _value = value;
//                 RaisePropertyChanged(() => Value);
//             }
//         }
//
//         public bool IsValid
//         {
//             get
//             {
//                 return _isValid;
//             }
//             set
//             {
//                 _isValid = value;
//                 RaisePropertyChanged(() => IsValid);
//             }
//         }
//
//         public ValidatableObject()
//         {
//             _isValid = true;
//             _errors = new List<string>();
//             _validations = new List<IValidationRule<T>>();
//             _IsVisible = false;
//         }
//
//         public bool Validate()        {
//             Errors.Clear();
//
//             IEnumerable<string> errors = _validations.Where(v => !v.Check(Value))
//                 .Select(v => v.ValidationMessage);
//
// 			Errors = errors.ToList();
//             IsValid = !Errors.Any();
//
//             IsVisible = !IsValid;
//
//
//             return this.IsValid;
//         }
//
//     }
