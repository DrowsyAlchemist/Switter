namespace UserService.Exceptions.Profiles
{
    public class UserNotFoundException : UserServiceException
    {
        public UserNotFoundException(Guid id) : base($"User profile with id \"{id}\" is not found.")
        {
        }
    }
}
