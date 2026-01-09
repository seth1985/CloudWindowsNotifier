using Xunit;
using WindowsNotifierCloud.Domain.Entities;
using System;

namespace WindowsNotifierCloud.Tests.Domain;

public class ModuleDefinitionTests
{
    [Fact]
    public void ModuleDefinition_Create_ShouldBeValid()
    {
        // Arrange
        var module = ModuleDefinition.Create("Test", "test-id", ModuleType.Standard, ModuleCategory.GeneralInfo, Guid.NewGuid());

        // Act & Assert
        Assert.Equal("Test", module.DisplayName);
        Assert.Equal("test-id", module.ModuleId);
        Assert.False(module.IsPublished);
    }
}
