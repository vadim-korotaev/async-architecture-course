create table task_statuses (
    id serial PRIMARY KEY,
    status_name varchar
);

INSERT INTO task_statuses (id, status_name) values
    (1, 'InProgress'),
    (2, 'Completed');

create table ates_tasks (
	id serial PRIMARY KEY,
	name varchar,
    description varchar,
    status_id int not null,
    assigned_user uuid not null,
    public_id uuid not null,

	CONSTRAINT ates_task_status_id_fkey FOREIGN KEY (status_id) REFERENCES task_statuses(id)
);

create table user_roles (
    id serial PRIMARY KEY,
    role_name varchar
);

INSERT INTO user_roles (id, role_name) values
    (1, 'Worker'),
    (2, 'Admin'),
    (3, 'Manager');

create table users (
    id serial PRIMARY KEY,
    username varchar,
    role_id int not null,
    is_deleted boolean default false,
    public_id uuid not null,

    CONSTRAINT user_role_id_fkey FOREIGN KEY (role_id) REFERENCES user_roles(id)
);
