using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.DataAccess;
using System.Web.Management;
using System.Web.Util;
using System.Web.Security;
using System.Linq;
using System.Linq.Expressions;
using A1fxCrm.Web.Framework.Model;
using A1fxCrm.Web.Framework.DataContexts;
 

namespace A1fxCrm.Web.Framework.Security
{
    public class EFMembershipProvider : System.Web.Security.MembershipProvider
    {
        private const int PASSWORD_SIZE = 14;
        private const int SALT_SIZE_IN_BYTES = 16;

        private int _SchemaVersionCheck;
        private bool _AutoInstall = true;
        private bool _ReturnEFMembershipUser = true;
        private bool _EnablePasswordRetrieval;
        private bool _EnablePasswordReset;
        private bool _RequiresQuestionAndAnswer;
        private bool _RequiresUniqueEmail;
        private int _MaxInvalidPasswordAttempts;
        private int _PasswordAttemptWindow;
        private int _MinRequiredPasswordLength;
        private int _MinRequiredNonalphanumericCharacters;
        private string _PasswordStrengthRegularExpression;
        private int _CommandTimeout;
        private MembershipPasswordFormat _PasswordFormat;
        // private string _sqlConnectionString;
        private static bool isCheck = false;
        private string s_HashAlgorithm;
        //This is for .Net 4.0
        private MembershipPasswordCompatibilityMode _LegacyPasswordCompatibilityMode;

        CommonContext db = new CommonContext();

        public User GetDBUser(string username)
        {

            return db.Users.FirstOrDefault(c => c.UserName == username);

        }

        public override void Initialize(string name, NameValueCollection config)
        {
           
            if (config == null)
                throw new ArgumentNullException("config");
  
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "MembershipSqlProvider_description");
            }
            base.Initialize(name, config);

            _SchemaVersionCheck = 0;

            _ReturnEFMembershipUser = SecUtility.GetBooleanValue(config, "returnEFMembershipUser", false);
            _AutoInstall = SecUtility.GetBooleanValue(config, "autoInstall", true);
            _EnablePasswordRetrieval = SecUtility.GetBooleanValue(config, "enablePasswordRetrieval", false);
            _EnablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", true);
            _RequiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            _RequiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", true);
            _MaxInvalidPasswordAttempts = SecUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            _PasswordAttemptWindow = SecUtility.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            _MinRequiredPasswordLength = SecUtility.GetIntValue(config, "minRequiredPasswordLength", 3, false, 128);
            _MinRequiredNonalphanumericCharacters = SecUtility.GetIntValue(config, "minRequiredNonalphanumericCharacters", 0, true, 128);

            _PasswordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if (_PasswordStrengthRegularExpression != null)
            {
                _PasswordStrengthRegularExpression = _PasswordStrengthRegularExpression.Trim();
                if (_PasswordStrengthRegularExpression.Length != 0)
                {
                    try
                    {
                        Regex regex = new Regex(_PasswordStrengthRegularExpression);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ProviderException(e.Message, e);
                    }
                }
            }
            else
            {
                _PasswordStrengthRegularExpression = string.Empty;
            }
            if (_MinRequiredNonalphanumericCharacters > _MinRequiredPasswordLength)
                throw new HttpException("MinRequiredNonalphanumericCharacters_can_not_be_more_than_MinRequiredPasswordLength");

            _CommandTimeout = SecUtility.GetIntValue(config, "commandTimeout", 30, true, 0);


            string strTemp = config["passwordFormat"];
            if (strTemp == null)
                strTemp = "Hashed";

            switch (strTemp)
            {
                case "Clear":
                    _PasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                case "Encrypted":
                    _PasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Hashed":
                    _PasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                default:
                    throw new ProviderException("Provider_bad_password_format");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed && EnablePasswordRetrieval)
                throw new ProviderException("Provider_can_not_retrieve_hashed_password");

            string passwordCompactMode = config["passwordCompatMode"];
            if (!string.IsNullOrEmpty(passwordCompactMode))
            {
                this._LegacyPasswordCompatibilityMode = (MembershipPasswordCompatibilityMode)Enum.Parse(typeof(MembershipPasswordCompatibilityMode), passwordCompactMode);
            }

            config.Remove("returnEFMembershipUser");
            config.Remove("autoInstall");
            config.Remove("returnEFMembershipUser");
            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("commandTimeout");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException("Provider_unrecognized_attribute, attribUnrecognized");
            }
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out User usr)
        {
            string salt;
            int passwordFormat;
            return CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved, out salt, out passwordFormat, out usr);
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat, out User usr)
        {
            var user = GetDBUser(username);

            usr = user;
            if (user == null)
            {
                salt = null;
                passwordFormat = -1;

                return false;
            }

            var enc = EncodePassword(password, user.PasswordFormat, user.PasswordSalt);
            passwordFormat = user.PasswordFormat;
            salt = user.PasswordSalt;
            if (enc == user.Password)
            {
                if (updateLastLoginActivityDate)
                {
                    user.LastActivityDate = DateTime.Now;
                    user.LastLoginDate = DateTime.Now;
                    db.SaveChanges();
                }
                return true;
            }
            else
                return false;
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            SecUtility.CheckParameter(ref oldPassword, true, true, false, 0x80, "oldPassword");
            SecUtility.CheckParameter(ref newPassword, true, true, false, 0x80, "newPassword");

            var salt = string.Empty;
            var passwordFormat = 1;


            var user = default(User);
            if (!CheckPassword(username, oldPassword, false, false, out salt, out passwordFormat, out user))
            {
                return false;
            }


            user.Password = EncodePassword(newPassword, passwordFormat, salt);
            user.LastPasswordChangedDate = DateTime.Now;
            user.FailedPasswordAnswerAttemptCount = 0;
            user.FailedPasswordAttemptCount = 0;
            db.SaveChanges();


            return true;
        }

        public bool ChangePasswordByAdmin(string username, string newPassword)
        {
           
            var passwordFormat = 1;
          
            var user = db.Users.First(c => c.UserName == username);
            var salt = user.PasswordSalt;
            user.Password = EncodePassword(newPassword, passwordFormat, salt);
            user.LastPasswordChangedDate = DateTime.Now;
            user.FailedPasswordAnswerAttemptCount = 0;
            user.FailedPasswordAttemptCount = 0;
            db.SaveChanges();

            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {

            var user = default(User);
            string salt; int passwordFormat;
            if (!CheckPassword(username, password, false, false, out salt, out passwordFormat, out user))
            {
                return false;
            }
            user.PasswordQuestion = newPasswordQuestion;
            user.PasswordAnswer = newPasswordAnswer;

            db.SaveChanges();

            return true;

        }

        public override System.Web.Security.MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out System.Web.Security.MembershipCreateStatus status)
        {
            if (!ValidateParameter(ref password, true, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            var salt = GenerateSalt();
            var pass = EncodePassword(password, (int)_PasswordFormat, salt);
            if (pass.Length > 128)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            string encodedPasswordAnswer;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }

            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                if (passwordAnswer.Length > 128)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)_PasswordFormat, salt);
            }
            else
                encodedPasswordAnswer = passwordAnswer;

            if (!ValidateParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            if (!ValidateParameter(ref username, true, true, true, 256))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if (!ValidateParameter(ref email,
                                               RequiresUniqueEmail,
                                               RequiresUniqueEmail,
                                               false,
                                               256))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if (!ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 256))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            if (providerUserKey != null)
            {
                //if (!(providerUserKey is Guid)) {
                //    status = MembershipCreateStatus.InvalidProviderUserKey;
                //    return null;
                //}
                status = MembershipCreateStatus.InvalidProviderUserKey;
                return null;
            }

            if (password.Length < MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            int count = 0;

            for (int i = 0; i < password.Length; i++)
            {
                if (!char.IsLetterOrDigit(password, i))
                {
                    count++;
                }
            }

            if (count < MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(password, PasswordStrengthRegularExpression))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }
            }


            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }


            if (RequiresUniqueEmail)
            {
                if (db.Users.Where(u => u.Email == email).Any())
                {
                    status = MembershipCreateStatus.DuplicateEmail;
                    return null;
                }
            }

            if (db.Users.Where(u => u.UserName == username).Any())
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            var utc = DateTime.UtcNow;
            var user = new User()
            {
                Comment = "",
                CreatedDate = utc,
                Email = email,
                FailedPasswordAnswerAttemptCount = 0,
                FailedPasswordAnswerAttemptWindowStart = utc,
                FailedPasswordAttemptCount = 0,
                FailedPasswordAttemptWindowStart = utc,
                IsAnonymous = false,
                IsApproved = isApproved,
                LastActivityDate = utc,
                LastLockoutDate = utc,
                LastLoginDate = utc,
                LastPasswordChangedDate = utc,
                Password = pass,
                PasswordAnswer = encodedPasswordAnswer,
                PasswordFormat = (int)PasswordFormat,
                PasswordQuestion = passwordQuestion,
                PasswordSalt = salt,
                TimeZone = 0,
                UserName = username,
            };

            db.Users.Add(user);
            try
            {
                db.SaveChanges();
            }
            catch(Exception exp)
            {
                status = MembershipCreateStatus.UserRejected;
                return null;
            }

            status = MembershipCreateStatus.Success;
            return UserMapper.Map(this.Name, user, _ReturnEFMembershipUser);

        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");


            var user = db.Users.Include("Roles").FirstOrDefault(u => u.UserName == username);

            if (user == null)
                return false;

            foreach (var role in user.Roles)
                user.Roles.Remove(role);

            db.Users.Remove(user);
            db.SaveChanges();

            return true;

        }

        public override bool EnablePasswordReset
        {
            get { return _EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _EnablePasswordRetrieval; }
        }

        public override System.Web.Security.MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref emailToMatch, false, false, false, 0x100, "emailToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException("PageIndex_bad", "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("PageSize_bad", "pageSize");
            }


            var query = from user in db.Users
                        where user.Email.Contains(emailToMatch)
                        orderby user.Id
                        select user;

            totalRecords = query.Count();
            var col = new MembershipUserCollection();

            foreach (var item in query.Skip(pageSize * pageIndex).Take(pageSize))
                col.Add(UserMapper.Map(this.Name, item, _ReturnEFMembershipUser));

            return col;

        }

        public override System.Web.Security.MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0x100, "usernameToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException("PageIndex_bad", "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("PageSize_bad", "pageSize");
            }


            var query = from user in db.Users
                        where user.UserName.Contains(usernameToMatch)
                        orderby user.Id
                        select user;

            totalRecords = query.Count();
            var col = new MembershipUserCollection();

            foreach (var item in query.Skip(pageSize * pageIndex).Take(pageSize))
                col.Add(UserMapper.Map(this.Name, item, _ReturnEFMembershipUser));

            return col;

        }

        public override System.Web.Security.MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException("PageIndex_bad", "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("PageSize_bad", "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException("PageIndex_PageSize_bad", "pageIndex and pageSize");
            }


            totalRecords = db.Users.Count();
            var col = new MembershipUserCollection();

            foreach (var item in db.Users.OrderBy(o => o.Id).Skip(pageSize * pageIndex).Take(pageSize))
                col.Add(UserMapper.Map(this.Name, item, _ReturnEFMembershipUser));

            return col;

        }

        public override int GetNumberOfUsersOnline()
        {

            return db.Users.Where(u => u.LastActivityDate > DateTime.Now.AddMinutes(Membership.UserIsOnlineTimeWindow)).Count();
        }

        public override string GetPassword(string username, string answer)
        {

            var usr = GetDBUser(username);
            if (db == null)
                return null;

            if (usr.PasswordAnswer == answer)
            {
                return UnEncodePassword(usr.Password, usr.PasswordFormat);
            }
            else
                return null;

        }

        public override System.Web.Security.MembershipUser GetUser(string username, bool userIsOnline)
        {

            var usr = GetDBUser(username);
            if (userIsOnline)
            {
                usr.LastActivityDate = DateTime.UtcNow;
                db.SaveChanges();
            }
            return UserMapper.Map(this.Name, usr, _ReturnEFMembershipUser);

        }

        public override System.Web.Security.MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }
            if (!(providerUserKey is int))
            {
                throw new ArgumentException("Membership_InvalidProviderUserKey", "providerUserKey");
            }



            var uid = (int)providerUserKey;
            var usr = db.Users.FirstOrDefault(u => u.Id == uid );
            if (usr == null) return null;
            if (userIsOnline)
            {
                usr.LastActivityDate = DateTime.UtcNow;
                db.SaveChanges();
            }
            return UserMapper.Map(this.Name, usr, _ReturnEFMembershipUser);

        }

        public override string GetUserNameByEmail(string email)
        {
            SecUtility.CheckParameter(ref email, false, false, false, 0x100, "email");


            var usr = (from u in db.Users where u.Email == email  select u.UserName).FirstOrDefault();
            return usr;

        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonalphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _PasswordAttemptWindow; }
        }

        public override System.Web.Security.MembershipPasswordFormat PasswordFormat
        {
            get { return _PasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _RequiresUniqueEmail; }
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Not_configured_to_support_password_resets");
            }

            SecUtility.CheckParameter(ref username, true, true, true, 256, "username");


            var user = GetDBUser(username);
            var passwordAnswer = answer;

            string encodedPasswordAnswer;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (!string.IsNullOrEmpty(passwordAnswer))
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), user.PasswordFormat, user.PasswordSalt);
            else
                encodedPasswordAnswer = passwordAnswer;

            SecUtility.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");
            string newPassword = GeneratePassword();

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ProviderException("Membership_Custom_Password_Validation_Failure");
                }
            }

            var utc = DateTime.UtcNow;
            if (!answer.Equals(user.PasswordAnswer, StringComparison.CurrentCultureIgnoreCase))
            {
                if (utc > user.FailedPasswordAnswerAttemptWindowStart.AddMinutes(PasswordAttemptWindow))
                {
                    user.FailedPasswordAnswerAttemptCount = 1;
                }
                else
                {
                    user.FailedPasswordAnswerAttemptCount++;
                }
                user.FailedPasswordAnswerAttemptWindowStart = utc;

                if (user.FailedPasswordAnswerAttemptCount > MaxInvalidPasswordAttempts)
                {
                    user.LastLockoutDate = DateTime.UtcNow;
                    user.Status = (byte)A1fxCrm.Web.Framework.Model.Enumerations.UserStatus.Locked;
                }

                db.SaveChanges();
                return null;
            }


            user.FailedPasswordAnswerAttemptCount = 0;
            user.FailedPasswordAnswerAttemptWindowStart = new DateTime(1754, 01, 01);

            user.FailedPasswordAttemptCount = 0;
            user.FailedPasswordAttemptWindowStart = user.FailedPasswordAnswerAttemptWindowStart;

            user.Password = EncodePassword(newPassword, user.PasswordFormat, user.PasswordSalt);
            db.SaveChanges();


            return newPassword;


        }

        public override bool UnlockUser(string userName)
        {
            SecUtility.CheckParameter(ref userName, true, true, true, 256, "username");
            try
            {
                var user = GetDBUser(userName);
                if (user == null)
                    return false;

                user.Status = (byte)A1fxCrm.Web.Framework.Model.Enumerations.UserStatus.Approved;
                user.LastLockoutDate = DateTime.UtcNow;

                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public override void UpdateUser(System.Web.Security.MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string temp = user.UserName;
            SecUtility.CheckParameter(ref temp, true, true, true, 256, "UserName");
            temp = user.Email;
            SecUtility.CheckParameter(ref temp,
                                      RequiresUniqueEmail,
                                      RequiresUniqueEmail,
                                      false,
                                      256,
                                      "Email");
            user.Email = temp;


            var query = from u in db.Users
                        where u.Id == (int)user.ProviderUserKey
                        select u;

            var usr = query.FirstOrDefault();
            if (usr == null)
                throw new ProviderException(GetExceptionText(1));

            if (RequiresUniqueEmail)
            {
                var q = from u in db.Users
                        where u.Id != (int)user.ProviderUserKey
                           && u.Email == user.Email 
                        select u;

                if (q.Any())
                    throw new ProviderException(GetExceptionText(7));
            }

            usr.Email = user.Email;
            usr.Comment = user.Comment;
            usr.IsApproved = user.IsApproved;
            usr.LastLoginDate = user.LastLoginDate;
            if (user is MembershipUser)
            {
                var muser = (MembershipUser)user;

                usr.FirstName = muser.FirstName;
                usr.LastName = muser.LastName;
            }

            db.SaveChanges();

        }

        public override bool ValidateUser(string username, string password)
        {

            var usr = default(User);
            if (ValidateParameter(ref username, true, true, true, 256) &&
                    ValidateParameter(ref password, true, true, false, 128) &&
                    CheckPassword(username, password, true, true, out usr))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private HashAlgorithm GetHashAlgorithm()
        {
            if (this.s_HashAlgorithm != null)
            {
                return HashAlgorithm.Create(this.s_HashAlgorithm);
            }
            string hashAlgorithmType = Membership.HashAlgorithmType;
            if (((this._LegacyPasswordCompatibilityMode == MembershipPasswordCompatibilityMode.Framework20)) && (hashAlgorithmType != "MD5"))
            {
                hashAlgorithmType = "SHA1";
            }
            HashAlgorithm algorithm = HashAlgorithm.Create(hashAlgorithmType);
            if (algorithm == null)
            {
                throw new ConfigurationErrorsException("Invalid_hash_algorithm_type");
            }
            this.s_HashAlgorithm = hashAlgorithmType;
            return algorithm;
        }

        internal string EncodePassword(string pass, int passwordFormat, string salt)
        {

            if (passwordFormat == 0)
            {
                return pass;
            }
            byte[] bytes = Encoding.Unicode.GetBytes(pass);
            byte[] src = Convert.FromBase64String(salt);
            byte[] inArray = null;
            if (passwordFormat == 1)
            {
                HashAlgorithm hashAlgorithm = this.GetHashAlgorithm();
                if (hashAlgorithm is KeyedHashAlgorithm)
                {
                    KeyedHashAlgorithm algorithm2 = (KeyedHashAlgorithm)hashAlgorithm;
                    if (algorithm2.Key.Length == src.Length)
                    {
                        algorithm2.Key = src;
                    }
                    else if (algorithm2.Key.Length < src.Length)
                    {
                        byte[] dst = new byte[algorithm2.Key.Length];
                        Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
                        algorithm2.Key = dst;
                    }
                    else
                    {
                        int num2;
                        byte[] buffer5 = new byte[algorithm2.Key.Length];
                        for (int i = 0; i < buffer5.Length; i += num2)
                        {
                            num2 = Math.Min(src.Length, buffer5.Length - i);
                            Buffer.BlockCopy(src, 0, buffer5, i, num2);
                        }
                        algorithm2.Key = buffer5;
                    }
                    inArray = algorithm2.ComputeHash(bytes);
                }
                else
                {
                    byte[] buffer6 = new byte[src.Length + bytes.Length];
                    Buffer.BlockCopy(src, 0, buffer6, 0, src.Length);
                    Buffer.BlockCopy(bytes, 0, buffer6, src.Length, bytes.Length);
                    inArray = hashAlgorithm.ComputeHash(buffer6);
                }
            }
            else
            {
                byte[] buffer7 = new byte[src.Length + bytes.Length];
                Buffer.BlockCopy(src, 0, buffer7, 0, src.Length);
                Buffer.BlockCopy(bytes, 0, buffer7, src.Length, bytes.Length);
                inArray = this.EncryptPassword(buffer7, _LegacyPasswordCompatibilityMode);
            }
            return Convert.ToBase64String(inArray);
        }

        internal string UnEncodePassword(string pass, int passwordFormat)
        {
            switch (passwordFormat)
            {
                case 0:
                    return pass;

                case 1:
                    throw new ProviderException("Provider_can_not_decode_hashed_password");
            }
            byte[] encodedPassword = Convert.FromBase64String(pass);
            byte[] bytes = this.DecryptPassword(encodedPassword);
            if (bytes == null)
            {
                return null;
            }
            return Encoding.Unicode.GetString(bytes, 0x10, bytes.Length - 0x10);

        }

        internal string GenerateSalt()
        {
            byte[] buf = new byte[SALT_SIZE_IN_BYTES];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword(
                      MinRequiredPasswordLength < PASSWORD_SIZE ? PASSWORD_SIZE : MinRequiredPasswordLength,
                      MinRequiredNonAlphanumericCharacters);
        }

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }

            param = param.Trim();
            if ((checkIfEmpty && param.Length < 1) ||
                 (maxSize > 0 && param.Length > maxSize) ||
                 (checkForCommas && param.Contains(",")))
            {
                return false;
            }

            return true;
        }

        private string GetExceptionText(int status)
        {
            string key;
            switch (status)
            {
                case 0:
                    return String.Empty;
                case 1:
                    key = "Membership_UserNotFound";
                    break;
                case 2:
                    key = "Membership_WrongPassword";
                    break;
                case 3:
                    key = "Membership_WrongAnswer";
                    break;
                case 4:
                    key = "Membership_InvalidPassword";
                    break;
                case 5:
                    key = "Membership_InvalidQuestion";
                    break;
                case 6:
                    key = "Membership_InvalidAnswer";
                    break;
                case 7:
                    key = "Membership_InvalidEmail";
                    break;
                case 99:
                    key = "Membership_AccountLockOut";
                    break;
                default:
                    key = "Provider_Error";
                    break;
            }

            return key;
        }

        private bool IsStatusDueToBadPassword(int status)
        {
            return (((status >= 2) && (status <= 6)) || (status == 0x63));
        }

        public override string ApplicationName
        {
            get
            {
               return string.Empty;
            }
            set { }
        }
    }
}
