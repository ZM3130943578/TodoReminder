using TodoReminder.Domain.Entities;

namespace TodoReminder.Tests.Domain;

public class TodoTaskTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var task = new TodoTask("Test Title", "Test Note");

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Test Title", task.Title);
        Assert.Equal("Test Note", task.Note);
        Assert.Null(task.DeletedAt);
        Assert.NotEqual(default, task.CreatedAt);
        Assert.NotEqual(default, task.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutNote_ShouldSetNoteNull()
    {
        var task = new TodoTask("Title Only", null);

        Assert.Equal("Title Only", task.Title);
        Assert.Null(task.Note);
    }

    [Fact]
    public void DefaultConstructor_ShouldGenerateId()
    {
        var task = new TodoTask();

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(string.Empty, task.Title);
        Assert.Null(task.Note);
    }
}
