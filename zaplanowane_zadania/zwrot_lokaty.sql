USE [msdb]
GO

/****** Object:  Job [zwrot_lokaty]    Script Date: 17.02.2021 09:27:06 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 17.02.2021 09:27:06 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'zwrot_lokaty', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'W momencie daty koñca lokaty jest ona zamykana a pieniadze wracaja na konto uzytkownika', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [zwrot]    Script Date: 17.02.2021 09:27:06 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'zwrot', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @id int
declare @kw money
declare @pr float
declare @ok int
declare @dat date
declare @id_kl int
declare @nr_k varchar(26) 
declare db_cursor cursor for
select id_lokaty,kwota,oprocentowanie,okres,data_rozpoczecia, id_klienta from lokaty where kwota > 0
open db_cursor
fetch next from db_cursor into @id,@kw,@pr,@ok,@dat,@id_kl
while @@FETCH_STATUS = 0
begin
	set @dat = DATEADD(month,@ok,@dat)
	if @dat < GETDATE()
	begin
		select top 1 @nr_k = numer_konta from Konta where id_klienta=@id_kl
		insert into Transakcje values (5,@kw,GETDATE(),''Zwrot lokaty'',''Wlasciciel lokaty'',6,''04175012822459556296178942'',@nr_k,''Bank Œwinka'',1)
		update Lokaty set kwota = 0 where id_lokaty= @id
	end	
	fetch next from db_cursor into @id,@kw,@pr,@ok,@dat,@id_kl
end
CLOSE db_cursor  
DEALLOCATE db_cursor', 
		@database_name=N'bank', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'zwrot_lokaty', 
		@enabled=1, 
		@freq_type=16, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20210201, 
		@active_end_date=99991231, 
		@active_start_time=11000, 
		@active_end_time=235959, 
		@schedule_uid=N'6e714dce-10b5-4336-b8f4-e6d8259850fb'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


