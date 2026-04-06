using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation;
using Mapster;  
using ECHO.Ticket.Core.DTOs;
using UserEntity = ECHO.Ticket.Core.Entities.User;

namespace ECHO.Ticket.Business.Services;

public class UserService : IUserService
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IValidator<UserEntity> _validator;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IRepository<UserEntity> userRepository, IValidator<UserEntity> validator, IJwtProvider jwtProvider, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _validator = validator;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<Result<string>> LoginAsync(UserLoginDto loginDto)
    {
        var users = await _userRepository.GetAllAsync(); 
        var user = users.FirstOrDefault(u => u.Email == loginDto.Email);

        if (user == null)
        {
            return Result<string>.Failure("Hata: Bu e-posta adresiyle kayıtlı bir kullanıcı bulunamadı.");
        }

        // 2. Şifreyi Kontrol Et (Şu an düz metin kontrol ediyoruz, ileride BCrypt ile şifreleyeceğiz)
        if (user.PasswordHash != loginDto.Password)
        {
            return Result<string>.Failure("Hata: Girdiğiniz şifre yanlış.");
        }

        // 3. Her şey doğruysa Token Fabrikasına Emir Ver
        var token = _jwtProvider.GenerateToken(user);

        // 4. Token'ı Başarı Mesajıyla Birlikte Geri Dön
        return Result<string>.Success(token, "Giriş başarılı! Token üretildi.");
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

    public async Task<Result> AddUserAsync(UserCreateDto userDto)
    {
        // 1. Mapster Sihri: DTO'yu User Entity'sine dönüştür
        var newUser = userDto.Adapt<UserEntity>();
        
        newUser.PasswordHash = _passwordHasher.HashPassword(userDto.PasswordHash);
        
        // DTO'da olmayan, bizim arka planda doldurmamız gereken değerler:
        newUser.CreatedAt = DateTime.UtcNow;
        newUser.IsActive = true;
        

        // 2. Validator Kontrolü (Artık Entity üzerinden çalışıyor)
        var validationResult = await _validator.ValidateAsync(newUser);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        // 3. E-posta Kontrolü
        var allUsers = await _userRepository.GetAllAsync();
        if (allUsers.Any(u => u.Email == newUser.Email))
            return Result.Failure("Bu e-posta adresi ile zaten bir kayıt mevcut!");

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return Result.Success("Kullanıcı başarıyla oluşturuldu.");
    }
    public async Task<Result> UpdateUserAsync(UserUpdateDto userDto)
    {
        var existingUser = await _userRepository.GetByIdAsync(userDto.Id);
        if (existingUser == null) return Result.Failure("Güncellenecek kullanıcı bulunamadı.");

        userDto.Adapt(existingUser);

        var validationResult = await _validator.ValidateAsync(existingUser);
        if (!validationResult.IsValid)
            return Result.Failure(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)));

        _userRepository.Update(existingUser);
        await _userRepository.SaveChangesAsync();
        return Result.Success("Kullanıcı başarıyla güncellendi.");
    }

    public async Task<Result> DeleteUserAsync(Guid id)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null) return Result.Failure("Silinecek kullanıcı bulunamadı.");

        _userRepository.Remove(existingUser);
        await _userRepository.SaveChangesAsync();
        return Result.Success("Kullanıcı başarıyla silindi.");
    }
}