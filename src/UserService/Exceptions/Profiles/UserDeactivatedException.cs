namespace UserService.Exceptions.Profiles
{
    public class UserDeactivatedException : UserServiceException
    {
        public UserDeactivatedException(Guid id) : base($"User profile with id \"{id}\" is deactivated.") { }
    }
}
