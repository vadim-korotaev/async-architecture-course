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
