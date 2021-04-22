USE [msdb]
GO

/****** Object:  Job [przelewy_zaplanowane]    Script Date: 17.02.2021 09:26:43 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 17.02.2021 09:26:43 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'przelewy_zaplanowane', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Przelew]    Script Date: 17.02.2021 09:26:43 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Przelew', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @od_kogo varchar(26)
declare @do_kogo varchar(26)
declare @kwota money
declare @data date
declare @id int
declare @cur money
declare @prowizja money
declare ms_cursor cursor for select od_kogo,do_kogo,kwota,data_transakcji,id_transakcji from transakcje where wykonane = 0 and data_transakcji <= GETDATE()
open ms_cursor
fetch ms_cursor into @od_kogo,@do_kogo,@kwota,@data,@id
WHILE @@FETCH_STATUS = 0
begin
	SET @cur = (select stan_konta from Konta where numer_konta = @od_kogo);
	if @@ROWCOUNT > 0
	begin
		set @prowizja = (select tk.prowizja from Konta k join Typy_Konta tk on k.typ_konta=tk.id_typu where k.numer_konta=@od_kogo )
		if @kwota+@prowizja < @cur
		begin
				update Konta set stan_konta = stan_konta - @kwota -@prowizja where numer_konta = @od_kogo
				update Konta set stan_konta = stan_konta + @kwota where numer_konta = @do_kogo
				update Transakcje set wykonane = 1 where id_transakcji=@id
		end
		else
		begin
		
		insert into powiadomienia values(''Zaplanowany przelew na konto ''+@do_kogo+ '' z numeru ''+ @od_kogo+'' na kwotê ''+@cur+'' nie móg³ zostaæ zrealizowany z powodu nie wystarczaj¹cego stanu konta'',@od_kogo)
		delete from transakcje where id_transakcji = @id
		end
	end
	else
	begin
		update Konta set stan_konta = stan_konta + @kwota where numer_konta = @do_kogo
		update Transakcje set wykonane = 1 where id_transakcji=@id
	end


	fetch ms_cursor into @od_kogo,@do_kogo,@kwota,@data,@id
end', 
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
		@active_start_date=20200909, 
		@active_end_date=99991231, 
		@active_start_time=20000, 
		@active_end_time=235959, 
		@schedule_uid=N'0f5370d2-4e34-49a5-8488-df748e447248'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


