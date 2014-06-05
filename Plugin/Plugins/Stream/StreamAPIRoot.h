#pragma once

class StreamAPIRoot
{
public:
	StreamAPIRoot();
	virtual ~StreamAPIRoot();

	StreamAPIRoot(StreamAPIRoot const&) = delete;
	StreamAPIRoot(StreamAPIRoot&&) = delete;
	StreamAPIRoot&	operator=(StreamAPIRoot const&) = delete;
	StreamAPIRoot&	operator=(StreamAPIRoot &&) = delete;

protected:
private:
};