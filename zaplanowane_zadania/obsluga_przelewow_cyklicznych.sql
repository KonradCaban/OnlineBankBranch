USE [msdb]
GO

/****** Object:  Job [obsluga_przelewow_cyklicznych]    Script Date: 17.02.2021 09:26:17 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 17.02.2021 09:26:17 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'obsluga_przelewow_cyklicznych', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Job obslugujacy przelewy cykliczne', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [obsluga_przelewow_cyklicznych]    Script Date: 17.02.2021 09:26:17 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'obsluga_przelewow_cyklicznych', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @kwota money
declare @tytul varchar(100)
declare @typ_transakcji int
declare @kategoria int
declare @od_kogo varchar(26)
declare @do_kogo varchar(26)
declare @dane_osobowe varchar(100)
declare @dane_nadawca varchar(100)
declare @co_ile int
declare @data date
declare @id int
declare db_cursor cursor for
select kwota,tytul,typ_transakcji,kategoria,od_kogo,do_kogo,dane_osobowe,dane_osobowe_nadawca,co_ile_dni,data,id_przelewu from Przelewy_Cykliczne where data<= Convert(date,GETDATE())
open db_cursor
fetch next from db_cursor into @kwota,@tytul,@typ_transakcji,@kategoria,@od_kogo,@do_kogo,@dane_osobowe,@dane_nadawca,@co_ile,@data,@id
while @@FETCH_STATUS = 0
begin
	insert into Transakcje values (@typ_transakcji,@kwota,GETDATE(),@tytul,@dane_osobowe,@kategoria,@od_kogo,@do_kogo,@dane_nadawca,1)
	update Przelewy_Cykliczne set data = DATEADD(day,@co_ile,@data) where id_przelewu=@id
	fetch next from db_cursor into @kwota,@tytul,@typ_transakcji,@kategoria,@od_kogo,@do_kogo,@dane_osobowe,@dane_nadawca,@co_ile,@data,@id
end
CLOSE db_cursor  
DEALLOCATE db_cursor', 
		@database_name=N'bank', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'przelewy_cykliczne', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20210106, 
		@active_end_date=99991231, 
		@active_start_time=34500, 
		@active_end_time=235959, 
		@schedule_uid=N'1eea538e-0ea7-4a2b-9e7d-06581d83c90d'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


