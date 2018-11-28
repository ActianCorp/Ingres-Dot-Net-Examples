create table dependents(
	name varchar(20) not null not default,
	depname varchar(15) not null not default,
	birth ingresdate not null not default
)
with noduplicates,
nojournaling,
	page_size = 8192,
location = (ii_database),
security_audit=(table,norow)
;
\p\g
modify dependents to btree on
	name,
	depname
with nonleaffill = 80,
	leaffill = 70,
	fillfactor = 80,
	extend = 16,
	page_size = 8192 ;
commit ;
\p\g
create table emp(
	name varchar(20) not null not default,
	title varchar(15) not null default ' ',
	hourly_rate money not null default 0,
	manager varchar(20)
)
with noduplicates,
nojournaling,
	page_size = 8192,
location = (ii_database),
security_audit=(table,norow)
;
\p\g
modify emp to btree on
	name
with nonleaffill = 80,
	leaffill = 70,
	fillfactor = 80,
	extend = 16,
	page_size = 8192
; commit ;
\p\g

insert into emp 
(name, title, hourly_rate, manager)
Values
('Alcot, Scott','Sr Programmer',50.00,'Wolfe, Neal');
insert into emp 
(name, title, hourly_rate, manager)
Values
('Applegate, Donald','Analyst',51.00,'Wolfe, Neal');
insert into emp 
(name, title, hourly_rate, manager)
Values
('Bee, Charles','Sr Programmer',43.00,'Fielding, Wallace');
insert into emp 
(name, title, hourly_rate, manager)
Values
('Belter, Kris','Programmer',33.00,'Alcott, Scott') ;
insert into dependents 
(name, depname, birth )
values 
('Alcott, Scott','Deborah','05-may-1986') ;
insert into dependents 
(name, depname, birth )
values 
('Alcott, Scott','Jason','08-jan-1971') ;
insert into dependents 
(name, depname, birth )
values 
('Bee, Charles','Chester','24-aug-1981') ;
insert into dependents 
(name, depname, birth )
values 
('Bee, Charles','Tom','10-jun-1983') ;
insert into dependents 
(name, depname, birth )
values 
('Belter, Kris','Alfred','26-sep-1988') ;
insert into dependents 
(name, depname, birth )
values 
('Belter, Kris','Janet','16-sep-1985') ;
insert into dependents 
(name, depname, birth )
values 
('Belter, Kris','Jennifer','08-jun-1988') ;
\p\g
/* create the database procedure */

create procedure rpp_proc  ( n_name=VARCHAR(20) not null)
result row ( VARCHAR(20) ,VARCHAR(15), VARCHAR(9) , DECIMAL(4,0) )
as
declare
NAME VARCHAR(20);
TITLE VARCHAR(15);
DEPNAME VARCHAR(9) ;
HOURLY_RATE DECIMAL(4,0) ;
begin
message 'START' with destination = (ERROR_LOG);
IF n_name <> ''
THEN
for  select e.name  , e.title , d.depname , e.hourly_rate
 into         :NAME , :TITLE , :DEPNAME , :HOURLY_RATE
from emp e , dependents d
where e.name  = d.name
        AND e.name = :n_name
do
        message 'ROW' with destination = (ERROR_LOG);
        return row (:NAME , :TITLE , :DEPNAME , :HOURLY_RATE  );

endfor;
ELSE
for
select e.name  , e.title , d.depname , e.hourly_rate
 into         :NAME , :TITLE , :DEPNAME , :HOURLY_RATE
from emp e , dependents d
where e.name  = d.name
do
        message 'join' with destination = (ERROR_LOG);
      return row ( :NAME , :TITLE , :DEPNAME , :HOURLY_RATE );
endfor;
endif ;
end  ; /* of dbproc */
commit ;
\p\g

grant execute on procedure rpp_proc to public ; 
\p\g
commit ;
\p\g
