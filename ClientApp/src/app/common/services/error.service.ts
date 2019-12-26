import { Injectable } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { HttpErrorResponse } from "@angular/common/http";

@Injectable()
export class ErrorService {

    constructor(
        private toastr: ToastrService
    ) { }

    public handleError(err: any, resourceType: string, action: string) {

        var title = `${action} ${resourceType} Error`;
        var message = `Failed to ${action} the ${resourceType}`;

        if (err instanceof HttpErrorResponse) {
            var httpError = <HttpErrorResponse>err;
            if (httpError.status === 0)
                message = "Unable to connect to the web server";
            else if (httpError.status === 400) {
                if (typeof err.error === "object") {
                    message = "";
                    for (var key in err.error) {
                        message += err.error[key] + "<br/>";
                    }
                } else {
                    message = err.error;
                }
            }
            else if (httpError.status === 403)
                // todo: reroute to home page?
                message = `You do not have permission to complete that action`;
            else if (httpError.status === 404)
                message = `The ${resourceType} could not be found`;
            else {
                if (err.message)
                    message = `${httpError.status}: ${err.message}`;
                else
                    message = `${httpError.status}: ${this.friendlyHttpStatus[httpError.status]}`;
            }
        } else {
            if (err.message) message = err.message;
        }

        console.log(err);
        this.toastr.error(message, title);

    }

    private friendlyHttpStatus = {
        200: 'OK',
        201: 'Created',
        202: 'Accepted',
        203: 'Non-Authoritative Information',
        204: 'No Content',
        205: 'Reset Content',
        206: 'Partial Content',
        300: 'Multiple Choices',
        301: 'Moved Permanently',
        302: 'Found',
        303: 'See Other',
        304: 'Not Modified',
        305: 'Use Proxy',
        306: 'Unused',
        307: 'Temporary Redirect',
        400: 'Bad Request',
        401: 'Unauthorized',
        402: 'Payment Required',
        403: 'Forbidden',
        404: 'Not Found',
        405: 'Method Not Allowed',
        406: 'Not Acceptable',
        407: 'Proxy Authentication Required',
        408: 'Request Timeout',
        409: 'Conflict',
        410: 'Gone',
        411: 'Length Required',
        412: 'Precondition Required',
        413: 'Request Entry Too Large',
        414: 'Request-URI Too Long',
        415: 'Unsupported Media Type',
        416: 'Requested Range Not Satisfiable',
        417: 'Expectation Failed',
        418: 'I\'m a teapot',
        500: 'Internal Server Error',
        501: 'Not Implemented',
        502: 'Bad Gateway',
        503: 'Service Unavailable',
        504: 'Gateway Timeout',
        505: 'HTTP Version Not Supported',
    };
}
