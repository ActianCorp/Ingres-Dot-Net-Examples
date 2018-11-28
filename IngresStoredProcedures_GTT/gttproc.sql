drop procedure gttproc;\p\g
create procedure gttproc (gtt1 set of (
seq integer,
tabname varchar(32),
tabowner varchar(32),
numpages integer)) as
declare cnt=integer not null;
tab=varchar(32) not null;
own=varchar(32) not null;
msg=varchar(200) not null;
begin
cnt=0;
for select tabname,tabowner into :tab, :own from gtt1 
  do
    cnt=:cnt+1;
    update gtt1 set seq=:cnt where tabname=:tab and tabowner=:own;
endfor;
msg='rows read=' + char(:cnt);
message :msg  with destination=(session);
end;
\p\g
