USE [msdb]
GO

/****** Object:  Job [zwiekszona_rata_za_opoznienie]    Script Date: 17.02.2021 09:26:53 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 17.02.2021 09:26:53 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'zwiekszona_rata_za_opoznienie', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Job wykonywany codziennie o 1:20, maj¹cy na celu na³o¿enie kary za nie sp³acenie raty kredytu w podanym terminie. Op³ata jest zwiêkszana o 0.05% za ka¿dy dzieñ zw³oki.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Op³ata]    Script Date: 17.02.2021 09:26:53 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Op³ata', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @id_raty int
declare @kwota_raty money
declare db_cursor cursor for
select id_raty,kwota_raty from Raty_kredytu where czy_splacona = 0 and termin_splaty < Convert(date,GETDATE())
open db_cursor
fetch next from db_cursor into @id_raty,@kwota_raty
while @@FETCH_STATUS = 0
begin
	update Raty_kredytu set kwota_raty=CEILING((@kwota_raty*1.0005)*100)/100 where id_raty=@id_raty
	fetch next from db_cursor into @id_raty,@kwota_raty
end
CLOSE db_cursor  
DEALLOCATE db_cursor', 
		@database_name=N'bank', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Codzienny', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20201206, 
		@active_end_date=99991231, 
		@active_start_time=12000, 
		@active_end_time=235959, 
		@schedule_uid=N'e6776256-dff0-47d4-878d-dfa3c4cacd6e'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


