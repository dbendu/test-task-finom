-- Скрипт для заполнения базы департаментами и сотрудниками

-- Create schema

create table deps (
    id serial primary key,
    name varchar(128) not null,
    active boolean not null default true
);

create table emps (
    id serial primary key,
    name varchar(128) not null,
    inn varchar(32) not null unique,
    departmentid integer references deps(id)
);

-- Generate data

insert into deps (name)
values
    ('HR'),
    ('Finance'),
    ('Marketing'),
    ('Sales'),
    ('IT'),
    ('Operations')
;

insert into emps (name, inn, departmentid)
select
    'Employee ' || i,
    '1000000000' || i,
    floor(random() * 6 + 1)::int
from generate_series(1, 20) AS s(i);