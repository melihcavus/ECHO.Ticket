using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation;
using UserEntity = ECHO.Ticket.Core.Entities.User;

namespace ECHO.Ticket.Business.Services;

public class UserService : IUserService
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IValidator<UserEntity> _validator;

    public UserService(IRepository<UserEntity> userRepository, IValidator<UserEntity> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Result<IEnumerable<UserEntity>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return Result<IEnumerable<UserEntity>>.Success(users);
    }

    public async Task<Result<UserEntity>> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            return Result<UserEntity>.Failure("Kullanıcı bulunamadı.");
            
        return Result<UserEntity>.Success(user);
    }

    public async Task<Result> AddUserAsync(UserEntity newUser)
    {
        // 1. FluentValidation Kontrolü (Email formatı doğru mu vb.)
        var validationResult = await _validator.ValidateAsync(newUser);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        // 2. Veritabanı Kontrolü (Email daha önce alınmış mı?)
        var allUsers = await _userRepository.GetAllAsync();
        if (allUsers.Any(u => u.Email == newUser.Email))
            return Result.Failure("Bu e-posta adresi ile zaten bir kayıt mevcut!");

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return Result.Success("Kullanıcı başarıyla oluşturuldu.");
    }
}