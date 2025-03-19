namespace user_management.Exceptions
{
    public class WrongCredentialException : System.Exception
    {
        public WrongCredentialException() : base("Wrong credentials provided.")
        {
        }

        public WrongCredentialException(string message) : base(message)
        {
        }

        public WrongCredentialException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToUpdateException : System.Exception
    {
        public FailToUpdateException() : base("Failed to update the record.")
        {
        }

        public FailToUpdateException(string message) : base(message)
        {
        }

        public FailToUpdateException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class InvalidModel : System.Exception
    {
        public InvalidModel() : base("Model is invalid.")
        {
        }

        public InvalidModel(string message) : base(message)
        {
        }

        public InvalidModel(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
    public class FailToDeactivateException : System.Exception
    {
        public FailToDeactivateException() : base("Failed to deactivate the record.")
        {
        }

        public FailToDeactivateException(string message) : base(message)
        {
        }

        public FailToDeactivateException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class AlreadyLogoutException : System.Exception
    {
        public AlreadyLogoutException() : base("User is already logged out.")
        {
        }

        public AlreadyLogoutException(string message) : base(message)
        {
        }

        public AlreadyLogoutException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class EmailAlreadyExistException : System.Exception
    {
        public EmailAlreadyExistException() : base("Email already exists.")
        {
        }

        public EmailAlreadyExistException(string message) : base(message)
        {
        }

        public EmailAlreadyExistException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToRegisterException : System.Exception
    {
        public FailToRegisterException() : base("Failed to register.")
        {
        }

        public FailToRegisterException(string message) : base(message)
        {
        }

        public FailToRegisterException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToRetrieveAccountInfoException : System.Exception
    {
        public FailToRetrieveAccountInfoException() : base("Failed to retrieve account information.")
        {
        }

        public FailToRetrieveAccountInfoException(string message) : base(message)
        {
        }

        public FailToRetrieveAccountInfoException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToRemoveException : System.Exception
    {
        public FailToRemoveException() : base("Failed to remove the record.")
        {
        }

        public FailToRemoveException(string message) : base(message)
        {
        }

        public FailToRemoveException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class UsernameAlreadyExistException : System.Exception
    {
        public UsernameAlreadyExistException() : base("Username already exists.")
        {
        }

        public UsernameAlreadyExistException(string message) : base(message)
        {
        }

        public UsernameAlreadyExistException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }

    public class FailToMeetCriteriaException : System.Exception
    {
        public FailToMeetCriteriaException() : base("Failed to meet criteria.")
        {
        }

        public FailToMeetCriteriaException(string message) : base(message)
        {
        }

        public FailToMeetCriteriaException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
}