import { environment } from '../../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Enum } from '../models/enums.model';
import { map } from 'rxjs/operators';
import { SearchQuery } from '../models/http.model';

@Injectable({ providedIn: 'root' })
export class DownloadService extends SearchQuery {

   constructor(private http: HttpClient) {
       super();
   }

   testReport(): Observable<DownloadModel> {
      return this.http.get<DownloadModel>(`${environment.baseApiUrl}downloads/test`, { responseType: 'blob' as 'json', observe: 'response' })
         .pipe(
            map(response => this.convertResponse(response))
         );
   }

   comparisonReport(municipalityId: string, sectorId: string, dateId: string, indicatorTypes: Enum[]): Observable<DownloadModel> {
      const queryParams: HttpParams = this.buildQueryParams({ indicatorTypes: indicatorTypes });
      return this.http.get<DownloadModel>(`${environment.baseApiUrl}downloads/comparison/${municipalityId}/${sectorId}/${dateId}`, { responseType: 'blob' as 'json', observe: 'response', params: queryParams })
         .pipe(
            map(response => this.convertResponse(response))
         );
   }

   private convertResponse(response: HttpResponse<any>): DownloadModel {
      let contentType = response.headers.get('Content-Type');
      let contentDispositionHeader = response.headers.get('Content-Disposition');
      let result = contentDispositionHeader.split(';')[1].trim().split('=')[1];
      let fileName = result.replace(/"/g, '');
      const file = <Blob>response.body;
      return <DownloadModel> { file: file, fileName: fileName, contentType: contentType };
   }

   downloadFile(download: DownloadModel) {
      // https://stackoverflow.com/a/52687792/214980
      // prepare data
      let binaryData = [];
      binaryData.push(download.file);
      var newBlob = new Blob(binaryData, { type: download.contentType });
      if (window.navigator && window.navigator.msSaveOrOpenBlob) {
         window.navigator.msSaveOrOpenBlob(newBlob);
         return;
      }
      const data = window.URL.createObjectURL(new Blob(binaryData, { type: download.contentType }));

      // create & click link
      let link = document.createElement('a');
      link.href = data;
      link.setAttribute('download', download.fileName);
      document.body.appendChild(link);
      link.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true, view: window }));

      // clean up
      link.parentNode.removeChild(link);
      setTimeout(function () {
         // For Firefox it is necessary to delay revoking the ObjectURL
         window.URL.revokeObjectURL(data);
         link.remove();
      }, 100);

   }
}

class DownloadModel {
   fileName: string;
   file: Blob;
   contentType: string;
}
