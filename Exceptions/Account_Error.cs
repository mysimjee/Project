
namespace user_management.Exceptions
{
    public class WrongCredentialException : Exception
    {
        public WrongCredentialException() : base("Wrong credentials provided.")
        {
        }

        public WrongCredentialException(string message) : base(message)
        {
        }

        public WrongCredentialException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToUpdateException : Exception
    {
        public FailToUpdateException() : base("Failed to update the record.")
        {
        }

        public FailToUpdateException(string message) : base(message)
        {
        }

        public FailToUpdateException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class InvalidModel : Exception
    {
        public InvalidModel() : base("Model is invalid.")
        {
        }

        public InvalidModel(string message) : base(message)
        {
        }

        public InvalidModel(string message, Exception inner) : base(message, inner)
        {
        }
    }
    public class FailToDeactivateException : Exception
    {
        public FailToDeactivateException() : base("Failed to deactivate the record.")
        {
        }

        public FailToDeactivateException(string message) : base(message)
        {
        }

        public FailToDeactivateException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class AlreadyLogoutException : Exception
    {
        public AlreadyLogoutException() : base("User is already logged out.")
        {
        }

        public AlreadyLogoutException(string message) : base(message)
        {
        }

        public AlreadyLogoutException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class EmailAlreadyExistException : Exception
    {
        public EmailAlreadyExistException() : base("Email already exists.")
        {
        }

        public EmailAlreadyExistException(string message) : base(message)
        {
        }

        public EmailAlreadyExistException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToRegisterException : Exception
    {
        public FailToRegisterException() : base("Failed to register.")
        {
        }

        public FailToRegisterException(string message) : base(message)
        {
        }

        public FailToRegisterException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToRetrieveAccountInfoException : Exception
    {
        public FailToRetrieveAccountInfoException() : base("Failed to retrieve account information.")
        {
        }

        public FailToRetrieveAccountInfoException(string message) : base(message)
        {
        }

        public FailToRetrieveAccountInfoException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToRemoveException : Exception
    {
        public FailToRemoveException() : base("Failed to remove the record.")
        {
        }

        public FailToRemoveException(string message) : base(message)
        {
        }

        public FailToRemoveException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class UsernameAlreadyExistException :Exception
    {
        public UsernameAlreadyExistException() : base("Username already exists.")
        {
        }

        public UsernameAlreadyExistException(string message) : base(message)
        {
        }

        public UsernameAlreadyExistException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToMeetCriteriaException :Exception
    {
        public FailToMeetCriteriaException() : base("Failed to meet criteria.")
        {
        }

        public FailToMeetCriteriaException(string message) : base(message)
        {
        }

        public FailToMeetCriteriaException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}