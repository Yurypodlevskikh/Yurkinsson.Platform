using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Services;
using FluentAssertions;
using Moq;
using DataAccess.Interfaces;
using Xunit;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metronome.Tests;

public class MetronomeSettingsServiceTests
{
    private readonly Mock<IMetronomeSettingsRepo> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MetronomeSettingsService _service;
    private readonly Mock<IUserInfoRepo> _userInfoRepoMock;

    public MetronomeSettingsServiceTests()
    {
        _repositoryMock = new Mock<IMetronomeSettingsRepo>();
        _mapperMock = new Mock<IMapper>();
        _userInfoRepoMock = new Mock<IUserInfoRepo>();
        _service = new MetronomeSettingsService(
            _mapperMock.Object, 
            _repositoryMock.Object, 
            _userInfoRepoMock.Object);
    }

    // Positive test case
    [Fact]
    public async Task GetSettingsByUserIdAsync_WithValidUserId_ReturnsMappedDtos()
    {
        // Arrange - prepare data, create mocks
        var userId = 1;

        // The list of entities from database
        var entities = new List<MetronomeSettings>
        {
            new MetronomeSettings
            {
                Id = 1,
                UserInfoId = userId,
                Title = "Warmup Session",
                SourceBpm = 80,
                TargetBpm = 120,
                BarsPerPattern = 4,
                TempoPeriods = 2,
                IsReversed = false,
                IsRepeatPattern = true,
                BeatsPerBar = 4,
                IsTempoIncreace = true,
                MetronomeCollectionId = 5,
                Description = "Test description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var expectedDtos = new List<MetronomeSettingsReadDto>
        {
            new MetronomeSettingsReadDto
            {
                Id = 1,
                Title = "Warmup Session",
                SourceBpm = 80,
                TargetBpm = 120,
                BarsPerPattern = 4,
                TempoPeriods = 2,
                IsReversed = false,
                IsRepeatPattern = true,
                BeatsPerBar = 4,
                IsTempoIncreace = true,
                MetronomeCollectionId = 5,
                Description = "Test description"
            }
        };

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByUserInfoIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        // Setup mapper mock to map entities to expedted DTOs
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(entities)).Returns(expectedDtos);

        // Act - call the service method
        var result = await _service.GetSettingsByUserIdAsync(userId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(expectedDtos);
        // Verify = check that the repository called the required number of times
    }

    // Negative test, should return empty list
    [Fact]
    public async Task GetSettingsByUserIdAsync_WithNoSettings_ReturnsEmptyList()
    {
        // Arrange - prepare data, create mocks
        var userId = 99;
        var emptyEntities = new List<MetronomeSettings>();

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByUserInfoIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(emptyEntities);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(emptyEntities)).Returns(new List<MetronomeSettingsReadDto>());

        // Act - call the service method
        var result = await _service.GetSettingsByUserIdAsync(userId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEmpty();
    }

    // Positive test case
    [Fact]
    public async Task GetSettingsByIdAsync_WithValidId_ReturnsMappedDto()
    {
        // Arrange - prepare data, create mocks
        var id = 1;
        var entity = new MetronomeSettings
        {
            Id = id,
            UserInfoId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedDto = new MetronomeSettingsReadDto
        {
            Id = id,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Setup mapper mock to map entities to expedted DTOs
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettingsReadDto>(entity)).Returns(expectedDto);

        // Act - call the service method
        var result = await _service.GetSettingsByIdAsync(id);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(expectedDto);
    }

    // Negative test, should return null
    [Fact]
    public async Task GetSettingsByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange - prepare data, create mocks
        var id = 99;

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null);

        // Act - call the service method
        var result = await _service.GetSettingsByIdAsync(id);

        // Assert - check the result using FluentAssertions
        result.Should().BeNull();
    }

    // Positive test case
    [Fact]
    public async Task CreateSettingsAsync_WithValidData_ReturnsReadDto()
    {
        using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Services;
using FluentAssertions;
using Moq;
using DataAccess.Interfaces;
using Xunit;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metronome.Tests;

public class MetronomeSettingsServiceTests
{
    private readonly Mock<IMetronomeSettingsRepo> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MetronomeSettingsService _service;
    private readonly Mock<IUserInfoRepo> _userInfoRepoMock;

    public MetronomeSettingsServiceTests()
    {
        _repositoryMock = new Mock<IMetronomeSettingsRepo>();
        _mapperMock = new Mock<IMapper>();
        _userInfoRepoMock = new Mock<IUserInfoRepo>();
        _service = new MetronomeSettingsService(
            _mapperMock.Object, 
            _repositoryMock.Object, 
            _userInfoRepoMock.Object);
    }

    // Positive test case
    [Fact]
    public async Task GetSettingsByUserIdAsync_WithValidUserId_ReturnsMappedDtos()
    {
        // Arrange - prepare data, create mocks
        var userId = 1;

        // The list of entities from database
        var entities = new List<MetronomeSettings>
        {
            new MetronomeSettings
            {
                Id = 1,
                UserInfoId = userId,
                Title = "Warmup Session",
                SourceBpm = 80,
                TargetBpm = 120,
                BarsPerPattern = 4,
                TempoPeriods = 2,
                IsReversed = false,
                IsRepeatPattern = true,
                BeatsPerBar = 4,
                IsTempoIncreace = true,
                MetronomeCollectionId = 5,
                Description = "Test description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var expectedDtos = new List<MetronomeSettingsReadDto>
        {
            new MetronomeSettingsReadDto
            {
                Id = 1,
                Title = "Warmup Session",
                SourceBpm = 80,
                TargetBpm = 120,
                BarsPerPattern = 4,
                TempoPeriods = 2,
                IsReversed = false,
                IsRepeatPattern = true,
                BeatsPerBar = 4,
                IsTempoIncreace = true,
                MetronomeCollectionId = 5,
                Description = "Test description"
            }
        };

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByUserInfoIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        // Setup mapper mock to map entities to expedted DTOs
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(entities)).Returns(expectedDtos);

        // Act - call the service method
        var result = await _service.GetSettingsByUserIdAsync(userId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(expectedDtos);
        // Verify = check that the repository called the required number of times
    }

    // Negative test, should return empty list
    [Fact]
    public async Task GetSettingsByUserIdAsync_WithNoSettings_ReturnsEmptyList()
    {
        // Arrange - prepare data, create mocks
        var userId = 99;
        var emptyEntities = new List<MetronomeSettings>();

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByUserInfoIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(emptyEntities);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(emptyEntities)).Returns(new List<MetronomeSettingsReadDto>());

        // Act - call the service method
        var result = await _service.GetSettingsByUserIdAsync(userId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEmpty();
    }

    // Positive test case
    [Fact]
    public async Task GetSettingsByIdAsync_WithValidId_ReturnsMappedDto()
    {
        // Arrange - prepare data, create mocks
        var id = 1;
        var entity = new MetronomeSettings
        {
            Id = id,
            UserInfoId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedDto = new MetronomeSettingsReadDto
        {
            Id = id,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Setup mapper mock to map entities to expedted DTOs
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettingsReadDto>(entity)).Returns(expectedDto);

        // Act - call the service method
        var result = await _service.GetSettingsByIdAsync(id);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(expectedDto);
    }

    // Negative test, should return null
    [Fact]
    public async Task GetSettingsByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange - prepare data, create mocks
        var id = 99;

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null);

        // Act - call the service method
        var result = await _service.GetSettingsByIdAsync(id);

        // Assert - check the result using FluentAssertions
        result.Should().BeNull();
    }

    // Positive test case
    [Fact]
    public async Task CreateSettingsAsync_WithValidData_ReturnsReadDto()
    {
        using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Services;
using FluentAssertions;
using Moq;
using DataAccess.Interfaces;
using Xunit;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metronome.Tests;

public class MetronomeSettingsServiceTests
{
    private readonly Mock<IMetronomeSettingsRepo> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MetronomeSettingsService _service;
    private readonly Mock<IUserInfoRepo> _userInfoRepoMock;

    public MetronomeSettingsServiceTests()
    {
        _repositoryMock = new Mock<IMetronomeSettingsRepo>();
        _mapperMock = new Mock<IMapper>();
        _userInfoRepoMock = new Mock<IUserInfoRepo>();
        _service = new MetronomeSettingsService(
            _mapperMock.Object, 
            _repositoryMock.Object, 
            _userInfoRepoMock.Object);
    }

    // Positive test case
    [Fact]
    public async Task GetSettingsByUserIdAsync_WithValidUserId_ReturnsMappedDtos()
    {
        // Arrange - prepare data, create mocks
        var userId = 1;

        // The list of entities from database
        var entities = new List<MetronomeSettings>
        {
            new MetronomeSettings
            {
                Id = 1,
                UserInfoId = userId,
                Title = "Warmup Session",
                SourceBpm = 80,
                TargetBpm = 120,
                BarsPerPattern = 4,
                TempoPeriods = 2,
                IsReversed = false,
                IsRepeatPattern = true,
                BeatsPerBar = 4,
                IsTempoIncreace = true,
                MetronomeCollectionId = 5,
                Description = "Test description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var expectedDtos = new List<MetronomeSettingsReadDto>
        {
            new MetronomeSettingsReadDto
            {
                Id = 1,
                Title = "Warmup Session",
                SourceBpm = 80,
                TargetBpm = 120,
                BarsPerPattern = 4,
                TempoPeriods = 2,
                IsReversed = false,
                IsRepeatPattern = true,
                BeatsPerBar = 4,
                IsTempoIncreace = true,
                MetronomeCollectionId = 5,
                Description = "Test description"
            }
        };

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByUserInfoIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        // Setup mapper mock to map entities to expedted DTOs
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(entities)).Returns(expectedDtos);

        // Act - call the service method
        var result = await _service.GetSettingsByUserIdAsync(userId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(expectedDtos);
        // Verify = check that the repository called the required number of times
    }

    // Negative test, should return empty list
    [Fact]
    public async Task GetSettingsByUserIdAsync_WithNoSettings_ReturnsEmptyList()
    {
        // Arrange - prepare data, create mocks
        var userId = 99;
        var emptyEntities = new List<MetronomeSettings>();

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByUserInfoIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(emptyEntities);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(emptyEntities)).Returns(new List<MetronomeSettingsReadDto>());

        // Act - call the service method
        var result = await _service.GetSettingsByUserIdAsync(userId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEmpty();
    }

    // Positive test case
    [Fact]
    public async Task GetSettingsByIdAsync_WithValidId_ReturnsMappedDto()
    {
        // Arrange - prepare data, create mocks
        var id = 1;
        var entity = new MetronomeSettings
        {
            Id = id,
            UserInfoId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedDto = new MetronomeSettingsReadDto
        {
            Id = id,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Setup mapper mock to map entities to expedted DTOs
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettingsReadDto>(entity)).Returns(expectedDto);

        // Act - call the service method
        var result = await _service.GetSettingsByIdAsync(id);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(expectedDto);
    }

    // Negative test, should return null
    [Fact]
    public async Task GetSettingsByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange - prepare data, create mocks
        var id = 99;

        // Setup repository mock to return predefined entities
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null);

        // Act - call the service method
        var result = await _service.GetSettingsByIdAsync(id);

        // Assert - check the result using FluentAssertions
        result.Should().BeNull();
    }

    // Positive test case
    [Fact]
    public async Task CreateSettingsAsync_WithValidData_ReturnsReadDto()
    {
        var guidId = "test-guid-123";
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            UserInfoId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var readDto = new MetronomeSettingsReadDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        _userInfoRepoMock.Setup(х => х.GetUserInfoByGuidIdAsync(guidId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserInfo { Id = 1 });
        // Setup mapper to map CreateDTO to Entity
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettings>(It.IsAny<MetronomeSettingsCreateDto>())).Returns(entity);

        // Setup repository to accept the new entity
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<MetronomeSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MetronomeSettings s, CancellationToken _) => s);

        // Setup mapper to map Entity to ReadDTO
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettingsReadDto>(It.IsAny<MetronomeSettings>())).Returns(readDto);

        // Act - call the service method
        var result = await _service.CreateSettingsAsync(createDto, guidId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(readDto);
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task CreateSettingsAsync_WithInvalidBpm_ThrowsArgumentException()
    {
        var guidId = "test-guid-123";
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 0,
            TargetBpm = 0,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        // Act - call the service method
        Func<Task> act = async () => await _service.CreateSettingsAsync(createDto, guidId);

        // Assert - check the result using FluentAssertions
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Source and target BPM must be greater than 0.");
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task CreateSettingsAsync_WhenRepositoryThrows_ExceptionIsPropagated()
    {
        var guidId = "test-guid-123";
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Setup mapper to map CreateDTO to Entity
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettings>(createDto)).Returns(entity);

        // Setup repository to throw an exception
        _repositoryMock.Setup(repo => repo.CreateAsync(entity, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Repository exception. (or Database error)"));

        // Act - call the service method
        Func<Task> act = async () => await _service.CreateSettingsAsync(createDto, guidId);

        // Assert - check the result using FluentAssertions
        await act.Should().ThrowAsync<Exception>().WithMessage("Repository exception. (or Database error)");
    }

    // Negative test, should returns false
    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsNotFound_ReturnsFalse()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null!);

        // Act
        var result = await _service.UpdateSettingsAsync(updateDto.Id, updateDto);

        // Assert
        result.Should().BeFalse();
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task UpdateSettingsAsync_WhenInvalidBpm_ThrowsArgumentException()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 0,
            TargetBpm = -5,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            Id = 1,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        Func<Task> act = async () => await _service.UpdateSettingsAsync(updateDto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Source and target BPM must be greater than 0.");
    }

    // Positive test case
    [Fact]
    public async Task UpdateSettingsAsync_WhenValid_ReturnsTrue()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            Id = 1,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        _mapperMock.Setup(mapper => mapper.Map(updateDto, entity));

        _repositoryMock.Setup(repo => repo.UpdateAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateSettingsAsync(updateDto);

        // Assert
        result.Should().BeTrue();
        _mapperMock.Verify(mapper => mapper.Map(updateDto, entity), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Positive case, should return true when delete is successful
    [Fact]
    public async Task DeleteSettingsAsync_WithExistingEntity_ReturnsTrue()
    {
        // Arrange
        var id = 1;

        var entity = new MetronomeSettings
        {
            Id = id,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSettingsAsync(id);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Negative case, should return false
    [Fact]
    public async Task DeleteSettingsAsync_WithNonExistingEntity_ReturnsFalse()
    {
        // Arrange
        var id = 1;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null);

        // Act
        var result = await _service.DeleteSettingsAsync(id);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Negative case, should return ThrowsException
    [Fact]
    public async Task DeleteSettingsAsync_WhenDeleteAsyncThrows_ThrowsException()
    {
        // Arrange
        var id = 1;

        var entity = new MetronomeSettings
        {
            Id = id,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Delet failed."));

        // Act
        Func<Task> action = async () => await _service.DeleteSettingsAsync(id);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Delet failed.");
    }
}

        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            UserInfoId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var readDto = new MetronomeSettingsReadDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        _userInfoRepoMock.Setup(repo => repo.GetByIdAsync(guidId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserInfo { Id = 1 });
        // Setup mapper to map CreateDTO to Entity
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettings>(It.IsAny<MetronomeSettingsCreateDto>())).Returns(entity);

        // Setup repository to accept the new entity
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<MetronomeSettings>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Setup mapper to map Entity to ReadDTO
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettingsReadDto>(entity)).Returns(readDto);

        // Act - call the service method
        var result = await _service.CreateSettingsAsync(createDto, guidId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(readDto);
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task CreateSettingsAsync_WithInvalidBpm_ThrowsArgumentException()
    {
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 0,
            TargetBpm = 0,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        // Act - call the service method
        Func<Task> act = async () => await _service.CreateSettingsAsync(createDto);

        // Assert - check the result using FluentAssertions
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Source and target BPM must be greater than 0.");
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task CreateSettingsAsync_WhenRepositoryThrows_ExceptionIsPropagated()
    {
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Setup mapper to map CreateDTO to Entity
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettings>(createDto)).Returns(entity);

        // Setup repository to throw an exception
        _repositoryMock.Setup(repo => repo.CreateAsync(entity, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Repository exception. (or Database error)"));

        // Act - call the service method
        Func<Task> act = async () => await _service.CreateSettingsAsync(createDto);

        // Assert - check the result using FluentAssertions
        await act.Should().ThrowAsync<Exception>().WithMessage("Repository exception. (or Database error)");
    }

    // Negative test, should returns false
    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsNotFound_ReturnsFalse()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null!);

        // Act
        var result = await _service.UpdateSettingsAsync(updateDto);

        // Assert
        result.Should().BeFalse();
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task UpdateSettingsAsync_WhenInvalidBpm_ThrowsArgumentException()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 0,
            TargetBpm = -5,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            Id = 1,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        Func<Task> act = async () => await _service.UpdateSettingsAsync(updateDto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Source and target BPM must be greater than 0.");
    }

    // Positive test case
    [Fact]
    public async Task UpdateSettingsAsync_WhenValid_ReturnsTrue()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            Id = 1,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        _mapperMock.Setup(mapper => mapper.Map(updateDto, entity));

        _repositoryMock.Setup(repo => repo.UpdateAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateSettingsAsync(updateDto);

        // Assert
        result.Should().BeTrue();
        _mapperMock.Verify(mapper => mapper.Map(updateDto, entity), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Positive case, should return true when delete is successful
    [Fact]
    public async Task DeleteSettingsAsync_WithExistingEntity_ReturnsTrue()
    {
        // Arrange
        var id = 1;

        var entity = new MetronomeSettings
        {
            Id = id,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSettingsAsync(id);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Negative case, should return false
    [Fact]
    public async Task DeleteSettingsAsync_WithNonExistingEntity_ReturnsFalse()
    {
        // Arrange
        var id = 1;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null);

        // Act
        var result = await _service.DeleteSettingsAsync(id);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Negative case, should return ThrowsException
    [Fact]
    public async Task DeleteSettingsAsync_WhenDeleteAsyncThrows_ThrowsException()
    {
        // Arrange
        var id = 1;

        var entity = new MetronomeSettings
        {
            Id = id,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Delet failed."));

        // Act
        Func<Task> action = async () => await _service.DeleteSettingsAsync(id);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Delet failed.");
    }
}

        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            UserInfoId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var readDto = new MetronomeSettingsReadDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        _userInfoRepoMock.Setup(repo => repo.GetByIdAsync(guidId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserInfo { Id = 1 });
        // Setup mapper to map CreateDTO to Entity
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettings>(It.IsAny<MetronomeSettingsCreateDto>())).Returns(entity);

        // Setup repository to accept the new entity
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<MetronomeSettings>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Setup mapper to map Entity to ReadDTO
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettingsReadDto>(entity)).Returns(readDto);

        // Act - call the service method
        var result = await _service.CreateSettingsAsync(createDto, guidId);

        // Assert - check the result using FluentAssertions
        result.Should().BeEquivalentTo(readDto);
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task CreateSettingsAsync_WithInvalidBpm_ThrowsArgumentException()
    {
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 0,
            TargetBpm = 0,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        // Act - call the service method
        Func<Task> act = async () => await _service.CreateSettingsAsync(createDto);

        // Assert - check the result using FluentAssertions
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Source and target BPM must be greater than 0.");
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task CreateSettingsAsync_WhenRepositoryThrows_ExceptionIsPropagated()
    {
        // Arrange - prepare data, create mocks
        var createDto = new MetronomeSettingsCreateDto
        {
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Setup mapper to map CreateDTO to Entity
        _mapperMock.Setup(mapper => mapper.Map<MetronomeSettings>(createDto)).Returns(entity);

        // Setup repository to throw an exception
        _repositoryMock.Setup(repo => repo.CreateAsync(entity, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Repository exception. (or Database error)"));

        // Act - call the service method
        Func<Task> act = async () => await _service.CreateSettingsAsync(createDto);

        // Assert - check the result using FluentAssertions
        await act.Should().ThrowAsync<Exception>().WithMessage("Repository exception. (or Database error)");
    }

    // Negative test, should returns false
    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsNotFound_ReturnsFalse()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null!);

        // Act
        var result = await _service.UpdateSettingsAsync(updateDto);

        // Assert
        result.Should().BeFalse();
    }

    // Negative test, should returns ArgumentException
    [Fact]
    public async Task UpdateSettingsAsync_WhenInvalidBpm_ThrowsArgumentException()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 0,
            TargetBpm = -5,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            Id = 1,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        // Act
        Func<Task> act = async () => await _service.UpdateSettingsAsync(updateDto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Source and target BPM must be greater than 0.");
    }

    // Positive test case
    [Fact]
    public async Task UpdateSettingsAsync_WhenValid_ReturnsTrue()
    {
        // Arrange
        var updateDto = new MetronomeSettingsUpdateDto
        {
            Id = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description"
        };

        var entity = new MetronomeSettings
        {
            Id = 1,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(updateDto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        _mapperMock.Setup(mapper => mapper.Map(updateDto, entity));

        _repositoryMock.Setup(repo => repo.UpdateAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateSettingsAsync(updateDto);

        // Assert
        result.Should().BeTrue();
        _mapperMock.Verify(mapper => mapper.Map(updateDto, entity), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Positive case, should return true when delete is successful
    [Fact]
    public async Task DeleteSettingsAsync_WithExistingEntity_ReturnsTrue()
    {
        // Arrange
        var id = 1;

        var entity = new MetronomeSettings
        {
            Id = id,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSettingsAsync(id);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Negative case, should return false
    [Fact]
    public async Task DeleteSettingsAsync_WithNonExistingEntity_ReturnsFalse()
    {
        // Arrange
        var id = 1;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((MetronomeSettings?)null);

        // Act
        var result = await _service.DeleteSettingsAsync(id);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Negative case, should return ThrowsException
    [Fact]
    public async Task DeleteSettingsAsync_WhenDeleteAsyncThrows_ThrowsException()
    {
        // Arrange
        var id = 1;

        var entity = new MetronomeSettings
        {
            Id = id,
            UserId = 1,
            Title = "Warmup Session",
            SourceBpm = 80,
            TargetBpm = 120,
            BarsPerPattern = 4,
            TempoPeriods = 2,
            IsReversed = false,
            IsRepeatPattern = true,
            BeatsPerBar = 4,
            IsTempoIncreace = true,
            MetronomeCollectionId = 5,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(repo => repo.DeleteAsync(id, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Delet failed."));

        // Act
        Func<Task> action = async () => await _service.DeleteSettingsAsync(id);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Delet failed.");
    }
}
